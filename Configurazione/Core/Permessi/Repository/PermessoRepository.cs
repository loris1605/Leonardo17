using Configurazione.Core.Context;
using Configurazione.Core.DTO;
using Microsoft.EntityFrameworkCore;
using Models.Repository;
using Models.Tables;

namespace Configurazione.Core.Repository
{
    public interface IPermessoRepository
    {
        Task<List<ConfigurazionePostazioneElencoDTO>> GetPermessi(int id, CancellationToken ctk = default);
        Task<bool> SavePermessi(int id, List<ConfigurazionePostazioneElencoDTO> postazioni, CancellationToken ctk = default);
    }

    public class PermessoRepository(IPermessoDbContext ctx) : BaseRepository<PermessoDbContext, Permesso>, IPermessoRepository
    {
        private readonly IPermessoDbContext _ctx = ctx;

        public async Task<List<ConfigurazionePostazioneElencoDTO>> GetPermessi(int id, CancellationToken ctk = default)
        {
            return await _ctx.Postazioni
                .AsNoTracking()
                .Select(ConfigurazionePostazioneElencoDTO.ToPostazioneElencoDto(id)) // Passi l'id qui
                .ToListAsync(ctk);
        }

        public async Task<bool> SavePermessi(int id, List<ConfigurazionePostazioneElencoDTO> postazioni, CancellationToken ctk = default)
        {
            var operatore = await _ctx.Operatori
                .Include(o => o.Permessi)
                .FirstOrDefaultAsync(o => o.Id == id, ctk);
            if (operatore == null)
                return false;
            // Rimuovi i permessi esistenti
            _ctx.Permessi.RemoveRange(operatore.Permessi);
            // Aggiungi i nuovi permessi
            foreach (var postazioneid in postazioni)
            {
                int postazioneId = postazioneid.Id;
                if (postazioneid.HasPermesso) operatore.Permessi.Add(new Permesso { PostazioneId = postazioneId, OperatoreId = id });
            }
            await _ctx.SaveChangesAsync(ctk);
            return true;
        }

        
    }
}
