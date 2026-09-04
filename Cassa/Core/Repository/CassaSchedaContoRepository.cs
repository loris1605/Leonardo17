using Cassa.Core.Context;
using Cassa.Core.DTO;
using Microsoft.EntityFrameworkCore;
using Models.Repository;
using Models.Tables;

namespace Cassa.Core.Repository
{
    public interface ICassaSchedaContoRepository
    {
        Task<List<CassaSchedaContoDTO>> GetSchedaContoBySchedaId(int schedaId, CancellationToken ctk = default);
    }

    public class CassaSchedaContoRepository : BaseRepository<CassaPostazioneDbContext, SchedaConto>,
        ICassaSchedaContoRepository
    {
        private readonly ICassaPostazioneDbContext _ctx;

        public CassaSchedaContoRepository(ICassaPostazioneDbContext ctx)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
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
