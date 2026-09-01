using Menu.Core.Context;
using Menu.Core.DTO;
using Microsoft.EntityFrameworkCore;
using Models.Repository;
using Models.Tables;
using System.Diagnostics;

namespace DTO.Repository
{
    public interface IMenuRepository
    {
        Task<List<MenuDTO>> CaricaPostazioniCassa(int CodiceOperatore, CancellationToken ctk = default);
        Task<bool> EsisteGiornataAperta(CancellationToken ctk = default);
        Task<bool> OpenGiornata(CancellationToken ctk = default);
    }

    public class MenuRepository : RepositoryBase<MenuDbContext, Permesso>, IMenuRepository
    {
        public MenuRepository() : base() { }
        public MenuRepository(Func<MenuDbContext> factory) : base(factory) { }

        public async Task<bool> EsisteGiornataAperta(CancellationToken ctk = default)
        {
            ctk.ThrowIfCancellationRequested();
            try
            {
                return await UsingContextAsync(ctx =>
                    ctx.Giornate.AsNoTracking().AnyAsync(p => p.Aperta, ctk));
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine(">>> [INFO] Operazione annullata dall'utente.");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> [ERROR] EsisteGiornataAperta: {ex.InnerException?.Message ?? ex.Message}");
                return false;
            }
        }

        public async Task<List<MenuDTO>> CaricaPostazioniCassa(int CodiceOperatore, CancellationToken ctk = default)
        {
            ctk.ThrowIfCancellationRequested();

            try
            {
                return await UsingContextAsync(async ctx =>
                {
                    IQueryable<Permesso> query =
                        ctx.Permessi
                            .AsNoTracking()
                            .Where(p => p.OperatoreId == CodiceOperatore)
                            .Where(p => p.Postazione != null && p.Postazione.TipoPostazioneId == 2)
                            .Where(p => p.PostazioneId > 0);

                    return await query.Select(MenuDTO.ToPermessoDTO).ToListAsync(ctk).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine(">>> [INFO] Operazione annullata dall'utente.");
                return [];
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> [ERROR] CaricaPostazioniCassa: {ex.InnerException?.Message ?? ex.Message}");
                return [];
            }
        }

        public async Task<bool> OpenGiornata(CancellationToken ctk = default)
        {
            ctk.ThrowIfCancellationRequested();

            try
            {
                return await UsingContextAsync(async ctx =>
                {
                    var giornata = new Giornata
                    {
                        Aperta = true,
                        DataInizio = DateTime.Now,
                        DataFine = DateTime.MaxValue
                    };

                    await ctx.Giornate.AddAsync(giornata, ctk).ConfigureAwait(false);
                    await ctx.SaveChangesAsync(ctk).ConfigureAwait(false);
                    return true;
                }).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine(">>> [INFO] Operazione annullata dall'utente.");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> [ERROR] OpenGiornata: {ex.InnerException?.Message ?? ex.Message}");
                return false;
            }
        }
    }
}