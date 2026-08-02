
using Microsoft.EntityFrameworkCore;
using Models.Interfaces;
using Models.Tables;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Models.Repository
{
    public interface IRepositoryBase<Ttable> where Ttable : class, IStandardTable, new()
    {
        Task<int> Add<TMap>(TMap map, CancellationToken ctk = default) where TMap : IMappable<Ttable>;
        Task<bool> Del(IMap map, CancellationToken ctk = default);
        void Dispose();
        Task<bool> EsisteNome(IMap dT, CancellationToken ctk = default);
        Task<bool> EsisteNomeUpd(IMap dT, CancellationToken ctk = default);
        Task<List<TMap>> GetAll<TMap>(Expression<Func<Ttable, TMap>> selector, Expression<Func<Ttable, bool>>? predicate = null, Expression<Func<Ttable, object>>? orderBy = null) where TMap : class, new();
        Task<List<TResult>> GetAll<TResult>(Expression<Func<Ttable, TResult>> selector, Expression<Func<Ttable, bool>>? predicate = null, CancellationToken ct = default);
        Task<TMap> GetById<TMap>(int id, Expression<Func<Ttable, TMap>> selector, CancellationToken ctk = default) where TMap : class, new();
        Task<bool> Upd<Tdto, DbTable>(Tdto dto, CancellationToken ctk = default)
                            where Tdto : IMappable<DbTable>, IMap
                            where DbTable : class, new();
    }

    public abstract class RepositoryBase<TContext, Ttable> : IRepositoryBase<Ttable>
        where TContext : DbContext, new()
        where Ttable : class, IStandardTable, new()
    {
        protected readonly Func<TContext> ContextFactory;

        public RepositoryBase() : this(() => new TContext())
        {
        }

        public RepositoryBase(Func<TContext> contextFactory)
        {
            ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            Debug.WriteLine($"***** [GC] {this.GetType().Name} {this.GetHashCode()} CARICATO *****");
        }

#if DEBUG
        ~RepositoryBase()
        {
            Debug.WriteLine($"***** [GC] {this.GetType().Name} {this.GetHashCode()} DISTRUTTO *****");
        }
#endif

        protected async Task<TResult> UsingContextAsync<TResult>(Func<TContext, Task<TResult>> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            using var ctx = ContextFactory();
            return await func(ctx).ConfigureAwait(false);
        }

        protected TResult UsingContext<TResult>(Func<TContext, TResult> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            using var ctx = ContextFactory();
            return func(ctx);
        }

        public async Task<bool> EsisteNome(IMap dT, CancellationToken ctk = default)
        {
            using var _ctx = ContextFactory();
            return await _ctx.Set<Ttable>().AnyAsync(p => p.Nome == dT.Nome, ctk).ConfigureAwait(false);
        }

        public async Task<List<TResult>> GetAll<TResult>(
                        Expression<Func<Ttable, TResult>> selector,
                        Expression<Func<Ttable, bool>>? predicate = null,
                        CancellationToken ct = default)
        {
            using var _ctx = ContextFactory();
            IQueryable<Ttable> query = _ctx.Set<Ttable>().AsNoTracking();

            if (predicate != null)
                query = query.Where(predicate);

            return await query.Select(selector).ToListAsync(ct).ConfigureAwait(false);
        }

        public async Task<bool> EsisteNomeUpd(IMap dT, CancellationToken ctk = default)
        {
            using var _ctx = ContextFactory();
            return await _ctx.Set<Ttable>().AnyAsync(p => p.Nome == dT.Nome && p.Id != dT.Id, ctk).ConfigureAwait(false);
        }

        public async Task<bool> Del(IMap map, CancellationToken ctk = default)
        {
            using var _ctx = ContextFactory();
            var row = await _ctx.Set<Ttable>().FindAsync([map.Id], ctk).ConfigureAwait(false);

            if (row == null) return false;

            _ctx.Set<Ttable>().Remove(row);

            try
            {
                await _ctx.SaveChangesAsync(ctk).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Errore Delete: {ex.Message}");
                return false;
            }
        }

        public virtual async Task<int> Add<TMap>(TMap map, CancellationToken ctk = default)
                            where TMap : IMappable<Ttable>
        {
            using var _ctx = ContextFactory();
            var entity = map.ToTable();
            await _ctx.Set<Ttable>().AddAsync(entity, ctk).ConfigureAwait(false);
            try
            {
                await _ctx.SaveChangesAsync(ctk).ConfigureAwait(false);
                return entity.Id;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Errore Add: {ex.InnerException?.Message ?? ex.Message}");
                return -1;
            }
        }

        public async Task<bool> Upd<Tdto, DbTable>(Tdto dto, CancellationToken ctk = default)
                            where Tdto : IMappable<DbTable>, IMap
                            where DbTable : class, new()
        {
            using var _ctx = ContextFactory();
            DbTable? entity = await _ctx.Set<DbTable>().FindAsync([dto.Id], ctk).ConfigureAwait(false);

            if (entity is null)
            {
                return false;
            }

            dto.UpdateTable(entity);

            await _ctx.SaveChangesAsync(ctk).ConfigureAwait(false);
            return true;
        }

        public virtual async Task<TMap> GetById<TMap>(int id, Expression<Func<Ttable, TMap>> selector,
                                                      CancellationToken ctk = default)
                                            where TMap : class, new()
        {
            using var _ctx = ContextFactory();

            var result = await _ctx.Set<Ttable>()
                                   .AsNoTracking()
                                   .Where(p => p.Id == id)
                                   .Select(selector)
                                   .FirstOrDefaultAsync(ctk).ConfigureAwait(false);

            return result ?? new TMap();
        }

        public async Task<List<TMap>> GetAll<TMap>(Expression<Func<Ttable, TMap>> selector,
                                                   Expression<Func<Ttable, bool>>? predicate = null,
                                                   Expression<Func<Ttable, object>>? orderBy = null)
                                            where TMap : class, new()
        {
            using var _ctx = ContextFactory();

            IQueryable<Ttable> query = _ctx.Set<Ttable>().AsNoTracking();

            if (predicate is not null)
            {
                query = query.Where(predicate);
            }

            if (orderBy is not null)
            {
                query = query.OrderBy(orderBy);
            }

            return await query.Select(selector).ToListAsync().ConfigureAwait(false);
        }

        public virtual void Dispose()
        {
#if DEBUG
            Debug.WriteLine($"***** [VM] {this.GetType().Name} {this.GetHashCode()} disposed *****");
#endif
        }
    }

    // Alias per retrocompatibilità con codice che usava il vecchio nome `BaseRepository`.
    
}