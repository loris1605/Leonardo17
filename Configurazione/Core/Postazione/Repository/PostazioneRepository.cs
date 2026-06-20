using Configurazione.Core.Context;
using Configurazione.Core.DTO;
using Microsoft.EntityFrameworkCore;
using Models.Repository;
using Models.Tables;
using System.Linq.Expressions;

namespace Configurazione.Core.Repository
{
    public interface IConfigurazionePostazioneRepository : IBaseRepository<Postazione>
    {
        Task<ConfigurazionePostazioneDTO> FirstPostazione(int id, CancellationToken ctk = default);
        Task<List<ConfigurazionePostazioneDTO>> Load(int id, CancellationToken ctk = default);
        Task<List<ConfigurazionePostazioneDTO>> LoadPostazioni(Expression<Func<Postazione, bool>> predicate, CancellationToken ctk = default);
        Task<List<ConfigurazioneTipoPostazioneDTO>> LoadTipiPostazione(CancellationToken ctk = default);
        Task<List<ConfigurazioneTipoRientroDTO>> LoadTipiRientro(CancellationToken ctk = default);
        Task<bool> Upd(ConfigurazionePostazioneDTO dto, CancellationToken ctk = default);
        //Task<bool> SaveReparti(int id, List<SettoreElencoDTO> settori, CancellationToken ctk = default);
        //Task<List<SettoreElencoDTO>> GetReparti(int id, CancellationToken ctk = default);
    }

    public class ConfigurazionePostazioneRepository(IPostazioneDbContext ctx) : 
                                            BaseRepository<PostazioneDbContext, Postazione>,
                                            IConfigurazionePostazioneRepository
    {
        private readonly IPostazioneDbContext _ctx = ctx;

        public async Task<List<ConfigurazionePostazioneDTO>> Load(int id, CancellationToken ctk = default)
        {
            if (id > 0)
                return await LoadPostazioni(x => x.Id == id, ctk);
            else
                return await LoadPostazioni(p => p.Id > -2, ctk);
        }

        public async Task<List<ConfigurazionePostazioneDTO>> LoadPostazioni(Expression<Func<Postazione, bool>> predicate
                                                            , CancellationToken ctk = default)
        {
            using PostazioneDbContext _ctx = new();

            // Carichiamo prima gli operatori con i loro dati (Eager Loading)
            return await _ctx.Postazioni
                .AsNoTracking()
                .Where(predicate)
                .OrderBy(o => o.Nome)
                .SelectMany(o => o.Reparti.DefaultIfEmpty(), ConfigurazionePostazioneDTO.ToPostazioniDtoRelationed) // <--- Usi l'espressione qui
                .ToListAsync(ctk);

        }

        public async Task<ConfigurazionePostazioneDTO> FirstPostazione(int id, CancellationToken ctk = default)
        {

            return await GetById(id, selector: ConfigurazionePostazioneDTO.ToPostazioneDto, ctk: ctk) ?? 
                    new ConfigurazionePostazioneDTO();

        }

        public async Task<bool> Upd(ConfigurazionePostazioneDTO dto, CancellationToken ctk = default)
        {
            return await Upd<ConfigurazionePostazioneDTO, Postazione>(dto, ctk);
        }

        public async Task<List<ConfigurazioneTipoPostazioneDTO>> LoadTipiPostazione(CancellationToken ctk = default)
        {
            using PostazioneDbContext _ctx = new();
            return await _ctx.TipiPostazione
                .AsNoTracking()
                .OrderBy(p => p.Nome)
                .Select(ConfigurazioneTipoPostazioneDTO.ToDto).ToListAsync(ctk);
        }

        public async Task<List<ConfigurazioneTipoRientroDTO>> LoadTipiRientro(CancellationToken ctk = default)
        {
            using PostazioneDbContext _ctx = new();
            return await _ctx.TipiRientro
                .AsNoTracking()
                .OrderBy(p => p.Nome)
                .Select(ConfigurazioneTipoRientroDTO.ToDto).ToListAsync(ctk);
        }

        //public async Task<List<SettoreElencoDTO>> GetReparti(int id, CancellationToken ctk = default)
        //{
        //    using PostazioneDbContext _ctx = new();
        //    return await _ctx.Settori.AsNoTracking()
        //        .Select(SettoreElencoDTO.ToSettoreElencoDto(id)) // Passi l'id qui
        //        .ToListAsync(ctk);
        //}

        //public async Task<bool> SaveReparti(int id, List<SettoreElencoDTO> settori, CancellationToken ctk = default)
        //{
        //    using PostazioneDbContext _ctx = new();
        //    var postazione = await _ctx.Postazioni
        //        .Include(o => o.Reparti)
        //        .FirstOrDefaultAsync(o => o.Id == id, ctk);
        //    if (postazione == null)
        //        return false;
        //    // Rimuovi i reparti esistenti
        //    _ctx.Reparti.RemoveRange(postazione.Reparti);
        //    // Aggiungi i nuovi reparti
        //    foreach (var settoredto in settori)
        //    {
        //        int settoreId = settoredto.Id;
        //        if (settoredto.HasReparto) postazione.Reparti.Add(new Reparto { SettoreId = settoreId, PostazioneId = id });
        //    }
        //    await _ctx.SaveChangesAsync(ctk);
        //    return true;
        //}
    }
}
