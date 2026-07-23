using Cassa.Core.Context;
using Cassa.Core.DTO;
using Microsoft.EntityFrameworkCore;
using Models.Repository;
using Models.Tables;

namespace Cassa.Core.Repository
{
    public interface ICassaPostazioneRepository
    {
        Task<string> GetPostazioneName(int id, CancellationToken ctk = default);
        Task<CassaSchedaDTO> GetSchedaByPosizione(string posizione, CancellationToken ctk = default);
    }

    public class CassaPostazioneRepository(ICassaPostazioneDbContext ctx) : 
                                    BaseRepository<CassaPostazioneDbContext, Postazione>, ICassaPostazioneRepository
    {
        public async Task<string> GetPostazioneName(int id, CancellationToken ctk = default)
        {
            var result = await ctx.Postazioni.Where(x => x.Id == id).FirstOrDefaultAsync(ctk);
            return result.Nome;
        }

        public async Task<CassaSchedaDTO> GetSchedaByPosizione(string posizione, CancellationToken ctk = default)
        {
            var result = await ctx.Schede
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

                    // Map other properties as needed
                })
                .FirstOrDefaultAsync(ctk);
            return result;
        }
    }


   

}
