using Cassa.Core.Context;
using Microsoft.EntityFrameworkCore;
using Models.Repository;
using Models.Tables;

namespace Cassa.Core.Repository
{
    public interface ICassaPostazioneRepository
    {
        Task<string> GetPostazioneName(int id, CancellationToken ctk = default);
    }

    public class CassaPostazioneRepository(ICassaPostazioneDbContext ctx) : 
                                    BaseRepository<CassaPostazioneDbContext, Postazione>, ICassaPostazioneRepository
    {
        public async Task<string> GetPostazioneName(int id, CancellationToken ctk = default)
        {
            var result = await ctx.Postazioni.Where(x => x.Id == id).FirstOrDefaultAsync(ctk);
            return result.Nome;
        }
    }

}
