using Configurazione.Core.Context;
using Configurazione.Core.DTO;
using Microsoft.EntityFrameworkCore;
using Models.Repository;
using Models.Tables;
using System.Linq.Expressions;

namespace Configurazione.Core.Repository
{
    public interface IConfigurazioneSettoreRepository : IBaseRepository<Settore>
    {
        Task<ConfigurazioneSettoreDTO> FirstSettore(int id, CancellationToken ctk = default);
        Task<List<ConfigurazioneSettoreDTO>> Load(int id, CancellationToken ctk = default);
        Task<List<ConfigurazioneSettoreDTO>> LoadSettori(Expression<Func<Settore, bool>> predicate, CancellationToken ctk = default);
        Task<bool> Upd(ConfigurazioneSettoreDTO dto, CancellationToken ctk = default);
        Task<List<ConfigurazioneTipoSettoreDTO>> LoadTipiSettore(CancellationToken ctk = default);
        //Task<List<TariffaDTO>> GetListini(int id, CancellationToken ctk = default);
        //Task<bool> SaveListini(int id, List<TariffaDTO> tariffe, CancellationToken ctk = default);
    }

    public class ConfigurazioneSettoreRepository(ISettoreDbContext ctx) : BaseRepository<SettoreDbContext, Settore>, IConfigurazioneSettoreRepository
    {
        private readonly ISettoreDbContext _ctx = ctx;

        public async Task<List<ConfigurazioneSettoreDTO>> Load(int id, CancellationToken ctk = default)
        {
            if (id > 0)
                return await LoadSettori(x => x.Id == id, ctk);
            else
                return await LoadSettori(p => p.Id > 0, ctk);
        }

        public async Task<List<ConfigurazioneSettoreDTO>> LoadSettori(Expression<Func<Settore, bool>> predicate
                                                            , CancellationToken ctk = default)
        {
            
            // Carichiamo prima gli operatori con i loro dati (Eager Loading)
            return await _ctx.Settori
                .AsNoTracking()
                .Where(predicate)
                .OrderBy(o => o.Nome)
                .SelectMany(o => o.Listini.DefaultIfEmpty(), ConfigurazioneSettoreDTO.ToSettoriDtoRelationed) // <--- Usi l'espressione qui
                .ToListAsync(ctk);

        }

        public async Task<List<ConfigurazioneTipoSettoreDTO>> LoadTipiSettore(CancellationToken ctk = default)
        {
            return await _ctx.TipiSettore
                .AsNoTracking()
                .OrderBy(p => p.Nome)
                .Select(ConfigurazioneTipoSettoreDTO.ToDto).ToListAsync(ctk);


        }

        public async Task<ConfigurazioneSettoreDTO> FirstSettore(int id, CancellationToken ctk = default) 
        {
            return await GetById(id, selector: ConfigurazioneSettoreDTO.ToSettoreDto, ctk: ctk) ?? 
                                new ConfigurazioneSettoreDTO();    
        }

        public async Task<bool> Upd(ConfigurazioneSettoreDTO dto, CancellationToken ctk = default) => 
                                await Upd<ConfigurazioneSettoreDTO, Settore>(dto, ctk);

        
    }
}
