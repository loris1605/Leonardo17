using Cassa.Core.Context;
using Cassa.Core.DTO;
using Microsoft.EntityFrameworkCore;
using Models.Repository;
using Models.Tables;
using System.Diagnostics;
using ViewModelServices.Core;

namespace Cassa.Core.Repository
{
    public interface ICassaPostazioneRepository
    {
        Task<string> GetPostazioneName(int id, CancellationToken ctk = default);
        Task<CassaSchedaDTO> GetSchedaByPosizione(string posizione, CancellationToken ctk = default);
        Task<List<CassaSchedaContoDTO>> GetSchedaContoBySchedaId(int schedaId, CancellationToken ctk = default);
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
            if (string.IsNullOrWhiteSpace(posizione))
                return new CassaSchedaDTO();

            try
            {
                var dto = await _ctx.Schede
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
                    Note = x.Note,
                    Conti = x.SchedeConto
                               .OrderBy(c => c.DataOra)
                               .Select(c => new CassaSchedaContoDTO
                               {
                                   Id = c.Id,
                                   CodiceScheda = c.SchedaId,
                                   DescSettore = c.DescSettore,
                                   DescPostazione = c.DescPostazione,
                                   VoiceDesc = c.VoiceDesc,
                                   VoicePrice = c.VoicePrice,
                                   Pagato = c.Pagato,
                                   Note = c.Note,
                                   DataOra = c.DataOra
                               })
                               .ToList()
                })
                .FirstOrDefaultAsync(ctk)
                .ConfigureAwait(false);

                return dto ?? new CassaSchedaDTO();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetSchedaByPosizione: {ex.Message}");
                return new CassaSchedaDTO();
            }

            
        }

        public async Task<List<CassaSchedaContoDTO>> GetSchedaContoBySchedaId(int schedaId, CancellationToken ctk = default)
        {
            ctk.ThrowIfCancellationRequested();

            // Verifica che il DbSet esista (evita eccezioni se il contesto non è stato aggiornato)
            if (_ctx.SchedeConto == null)
                return new List<CassaSchedaContoDTO>();

            var query = _ctx.SchedeConto
                .AsNoTracking()
                .Where(x => x.SchedaId == schedaId)
                .OrderBy(x => x.DataOra)
                .Select(x => new CassaSchedaContoDTO
                {
                    Id = x.Id,
                    CodiceScheda = x.SchedaId,
                    DescSettore = x.DescSettore,
                    DescPostazione = x.DescPostazione,
                    VoiceDesc = x.VoiceDesc,
                    VoicePrice = x.VoicePrice,
                    Pagato = x.Pagato,
                    Note = x.Note,
                    DataOra = x.DataOra
                });

            var result = await query.ToListAsync(ctk).ConfigureAwait(false);

            return result ?? new List<CassaSchedaContoDTO>();
        }
    }
}
