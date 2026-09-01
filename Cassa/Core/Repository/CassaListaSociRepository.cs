using Cassa.Core.Context;
using Cassa.Core.DTO;
using Microsoft.EntityFrameworkCore;
using Models.Repository;
using Models.Tables;

namespace Cassa.Core.Repository
{
    public interface ICassaListaSociRepository
    {
        Task<string> GetPostazioneName(int id, CancellationToken ctk = default);
        Task<IList<CassaSchedaDTO>> Load(CancellationToken ctk = default);
    }

    public class CassaListaSociRepository(ICassaPostazioneDbContext ctx) : BaseRepository<CassaPostazioneDbContext, Scheda>, ICassaListaSociRepository
    {
        public async Task<string> GetPostazioneName(int id, CancellationToken ctk = default)
        {
            var result = await ctx.Postazioni.Where(x => x.Id == id).FirstOrDefaultAsync(ctk);
            return result.Nome;
        }

        public async Task<IList<CassaSchedaDTO>> Load(CancellationToken ctk = default)
        {
            // Estrai le entità dal DB (AsNoTracking migliora le prestazioni in lettura)
            var schede = await ctx.Schede
                .AsNoTracking()
                .OrderBy(x => x.Posizione)
                .ToListAsync(ctk);

            // Mappa le entità nel tuo DTO usando il costruttore che hai definito
            var result = schede.Select(s => new CassaSchedaDTO(s)).ToList();

            return result;
        }
    }
}
