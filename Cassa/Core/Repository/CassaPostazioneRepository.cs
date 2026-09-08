using Cassa.Core.Context;
using Cassa.Core.DTO;
using Microsoft.EntityFrameworkCore;
using Models.Repository;
using Models.Tables;
using System.Diagnostics;

namespace Cassa.Core.Repository
{
    public interface ICassaPostazioneRepository : IBaseRepository<Postazione>
    {
        Task<string> GetPostazioneName(int id, CancellationToken ctk = default);
        Task<CassaSchedaDTO> GetSchedaByPosizione(string posizione, CancellationToken ctk = default);
        
    }

    public class CassaPostazioneRepository : BaseRepository<CassaPostazioneDbContext, Postazione>, ICassaPostazioneRepository
    {
        private readonly ICassaPostazioneDbContext _ctx;

        public CassaPostazioneRepository(ICassaPostazioneDbContext ctx)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        }

        public async Task<string> GetPostazioneName(int id, CancellationToken ctk = default)
        {
            ctk.ThrowIfCancellationRequested();

            var result = await _ctx.Postazioni
                                   .AsNoTracking()
                                   .Where(x => x.Id == id)
                                   .Select(x => x.Nome)
                                   .FirstOrDefaultAsync(ctk)
                                   .ConfigureAwait(false);

            return result ?? string.Empty;
        }

        public async Task<CassaSchedaDTO> GetSchedaByPosizione(string posizione, CancellationToken ctk = default)
        {
            ctk.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(posizione))
                return new CassaSchedaDTO();

            try
            {
                // Proiezione unica che include anche i conti (riduce round-trip al DB)
                var scheda = await _ctx.Schede
                    .AsNoTracking()
                    .Where(x => x.Posizione == posizione)
                    .Select(x => new CassaSchedaDTO
                    {
                        Id = x.Id,
                        Posizione = x.Posizione,
                        NumeroTessera = x.NumeroTessera,
                        CodicePerson = x.PersonId,
                        Cognome = x.Cognome,
                        Nome = x.Nome,
                        Natoil = x.Natoil,
                        CheckinTime = x.CheckinTime,
                        CheckoutTime = x.CheckoutTime,
                        Grb1 = x.Grb1,
                        Grb2 = x.Grb2,
                        Grb3 = x.Grb3,
                        Grb4 = x.Grb4,
                        Consumazione = x.Consumazione,
                        Blocco = x.Blocco,
                        Note = x.Note
                       
                    })
                    .FirstOrDefaultAsync(ctk)
                    .ConfigureAwait(false);

                return scheda ?? new CassaSchedaDTO();
            }
            catch (OperationCanceledException)
            {
                // preserva il comportamento di cancellazione
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetSchedaByPosizione: {ex}");
                return new CassaSchedaDTO();
            }
        }

        
    }
}
