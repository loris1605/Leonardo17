using Configurazione.Core.Context;
using Configurazione.Core.DTO;
using Microsoft.EntityFrameworkCore;
using Models.Repository;
using Models.Tables;

namespace Configurazione.Core.Repository
{
    public interface IConfigurazioneRepartoRepository
    {
        Task<List<ConfigurazioneSettoreElencoDTO>> GetReparti(int id, CancellationToken ctk = default);
        Task<bool> SaveReparti(int id, List<ConfigurazioneSettoreElencoDTO> settori, CancellationToken ctk = default);
    }

    public class ConfigurazioneRepartoRepository(IRepartoDbContext ctx) : BaseRepository<RepartoDbContext, Reparto>, IConfigurazioneRepartoRepository
    {
        private readonly IRepartoDbContext _ctx = ctx;

        public async Task<List<ConfigurazioneSettoreElencoDTO>> GetReparti(int id, CancellationToken ctk = default)
        {
            return await _ctx.Settori.AsNoTracking()
                .Select(ConfigurazioneSettoreElencoDTO.ToSettoreElencoDto(id)) // Passi l'id qui
                .ToListAsync(ctk);
        }

        public async Task<bool> SaveReparti(int id, List<ConfigurazioneSettoreElencoDTO> settori, CancellationToken ctk = default)
        {
            var postazione = await _ctx.Postazioni
                .Include(o => o.Reparti)
                .FirstOrDefaultAsync(o => o.Id == id, ctk);
            if (postazione == null)
                return false;
            // Rimuovi i reparti esistenti
            _ctx.Reparti.RemoveRange(postazione.Reparti);
            // Aggiungi i nuovi reparti
            foreach (var settoredto in settori)
            {
                int settoreId = settoredto.Id;
                if (settoredto.HasReparto) postazione.Reparti.Add(new Reparto { SettoreId = settoreId, PostazioneId = id });
            }
            await _ctx.SaveChangesAsync(ctk);
            return true;
        }
    }
}
