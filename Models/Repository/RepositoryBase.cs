csharp Models\Repository\RepositoryBase.cs
using Microsoft.EntityFrameworkCore;
using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Models.Repository
{
    /// <summary>
    /// Repository base con factory di DbContext (fallback al new TContext()).
    /// Fornisce implementazioni sicure e cancellabili per operazioni comuni.
    /// </summary>
    public abstract class RepositoryBase<TContext, TTable>
        where TContext : DbContext, new()
        where TTable : class, IStandardTable
    {
        private readonly Func<TContext> _contextFactory;

        protected RepositoryBase(Func<TContext>? contextFactory = null)
        {
            // Se non viene fornita una factory, usiamo il costruttore di default del DbContext.
            _contextFactory = contextFactory ?? (() => new TContext());
            Debug.WriteLine($"***** [GC] {GetType().Name} {GetHashCode()} CARICATO *****");
        }

#if DEBUG
        ~RepositoryBase()
        {
            Debug.WriteLine($"***** [GC] {GetType().Name} {GetHashCode()} DISTRUTTO *****");
        }
#endif

        protected TContext CreateContext() => _contextFactory();

        protected async Task<TResult> UsingContextAsync<TResult>(Func<TContext, Task<TResult>> action)
        {
            await using var ctx = CreateContext();
            return await action(ctx).ConfigureAwait(false);
        }

        protected async Task UsingContextAsync(Func<TContext, Task> action)
        {
            await using var ctx = CreateContext();
            await action(ctx).ConfigureAwait(false);
        }

        public virtual async Task<bool> EsisteNome(IMap dT, CancellationToken ctk = default)
        {
            return await UsingContextAsync(ctx =>
                ctx.Set<TTable>().AsNoTracking().AnyAsync(p => p.Nome == dT.Nome, ctk));
        }

        public virtual async Task<bool> EsisteNomeUpd(IMap dT, CancellationToken ctk = default)
        {
            return await UsingContextAsync(ctx =>
                ctx.Set<TTable>().AsNoTracking().AnyAsync(p => p.Nome == dT.Nome && p.Id != dT.Id, ctk));
        }

        public virtual async Task<List<TResult>> GetAll<TResult>(
            Expression<Func<TTable, TResult>> selector,
            Expression<Func<TTable, bool>>? predicate = null,
            CancellationToken ct = default)
        {
            return await UsingContextAsync(async ctx =>
            {
                IQueryable<TTable> query = ctx.Set<TTable>().AsNoTracking();

                if (predicate is not null)
                    query = query.Where(predicate);

                return await query.Select(selector).ToListAsync(ct).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public virtual async Task<List<TMap>> GetAll<TMap>(
            Expression<Func<TTable, TMap>> selector,
            Expression<Func<TTable, bool>>? predicate = null,
            Expression<Func<TTable, object>>? orderBy = null,
            CancellationToken ct = default)
            where TMap : class, new()
        {
            return await UsingContextAsync(async ctx =>
            {
                IQueryable<TTable> query = ctx.Set<TTable>().AsNoTracking();

                if (predicate is not null)
                    query = query.Where(predicate);

                if (orderBy is not null)
                    query = query.OrderBy(orderBy);

                return await query.Select(selector).ToListAsync(ct).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public virtual async Task<TMap> GetById<TMap>(int id, Expression<Func<TTable, TMap>> selector, CancellationToken ctk = default)
            where TMap : class, new()
        {
            return await UsingContextAsync(async ctx =>
            {
                var result = await ctx.Set<TTable>()
                                      .AsNoTracking()
                                      .Where(p => p.Id == id)
                                      .Select(selector)
                                      .FirstOrDefaultAsync(ctk)
                                      .ConfigureAwait(false);

                return result ?? new TMap();
            }).ConfigureAwait(false);
        }

        public virtual async Task<int> Add<TMap>(TMap map, CancellationToken ctk = default)
            where TMap : IMappable<TTable>
        {
            return await UsingContextAsync(async ctx =>
            {
                var entity = map.ToTable();
                await ctx.Set<TTable>().AddAsync(entity, ctk).ConfigureAwait(false);

                try
                {
                    await ctx.SaveChangesAsync(ctk).ConfigureAwait(false);
                    return entity.Id;
                }
                catch (OperationCanceledException)
                {
                    // Rilancia in modo che il chiamante possa gestire la cancellazione
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Errore Add: {ex.InnerException?.Message ?? ex.Message}");
                    return -1;
                }
            }).ConfigureAwait(false);
        }

        public virtual async Task<bool> Del(IMap map, CancellationToken ctk = default)
        {
            return await UsingContextAsync(async ctx =>
            {
                var row = await ctx.Set<TTable>().FirstOrDefaultAsync(p => p.Id == map.Id, ctk).ConfigureAwait(false);

                if (row is null)
                    return false;

                ctx.Set<TTable>().Remove(row);

                try
                {
                    await ctx.SaveChangesAsync(ctk).ConfigureAwait(false);
                    return true;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Errore Delete: {ex.InnerException?.Message ?? ex.Message}");
                    return false;
                }
            }).ConfigureAwait(false);
        }

        public virtual async Task<bool> Upd<TDto, TDbTable>(TDto dto, CancellationToken ctk = default)
            where TDto : IMappable<TDbTable>, IMap
            where TDbTable : class, new()
        {
            return await UsingContextAsync(async ctx =>
            {
                var set = ctx.Set<TDbTable>();

                var entity = await set.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == dto.Id, ctk).ConfigureAwait(false);

                if (entity is null)
                {
                    return false;
                }

                dto.UpdateTable(entity);

                try
                {
                    await ctx.SaveChangesAsync(ctk).ConfigureAwait(false);
                    return true;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Errore Upd: {ex.InnerException?.Message ?? ex.Message}");
                    return false;
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispose virtuale per eventuali override; non dispone il DbContext perché lo creiamo per chiamata.
        /// </summary>
        public virtual void Dispose()
        {
#if DEBUG
            Debug.WriteLine($"***** [Repository] {GetType().Name} {GetHashCode()} disposed *****");
#endif
        }
    }
}