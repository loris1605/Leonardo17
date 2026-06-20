using Configurazione.Core.Context;
using Configurazione.Core.DTO;
using Microsoft.EntityFrameworkCore;
using Models.Repository;
using Models.Tables;
using System.Linq.Expressions;

namespace Configurazione.Core.Repository
{
    public interface IConfigurazioneTariffaRepository : IBaseRepository<Tariffa>
    {
        Task<ConfigurazioneTariffaDTO> FirstTariffa(int id);
        Task<List<ConfigurazioneTariffaDTO>> Load(int id, CancellationToken ctk = default);
        Task<bool> Upd(ConfigurazioneTariffaDTO dto, CancellationToken ctk = default);
    }

    public class ConfigurazioneTariffaRepository(ITariffaDbContext ctx) : BaseRepository<TariffaDbContext, Tariffa>, IConfigurazioneTariffaRepository
    {
        private readonly ITariffaDbContext _ctx = ctx;

        public async Task<List<ConfigurazioneTariffaDTO>> Load(int id, CancellationToken ctk = default)
        {
            if (id > 0)
                return await LoadTariffe(x => x.Id == id, ctk);
            else
                return await LoadTariffe(p => p.Id > 0, ctk);
        }

        private async Task<List<ConfigurazioneTariffaDTO>> LoadTariffe(Expression<Func<Tariffa, bool>> predicate
                                                            , CancellationToken ctk = default)
        {
            
            // Carichiamo prima gli operatori con i loro dati (Eager Loading)
            return await _ctx.Tariffe
                .AsNoTracking()
                .Where(predicate)
                .OrderBy(o => o.Nome)
                .Select(ConfigurazioneTariffaDTO.ToDto) // <--- Usi l'espressione qui
                .ToListAsync(ctk);

        }

        public async Task<ConfigurazioneTariffaDTO> FirstTariffa(int id) => await GetById(id, selector: ConfigurazioneTariffaDTO.ToDto) ?? new ConfigurazioneTariffaDTO();

        public async Task<bool> Upd(ConfigurazioneTariffaDTO dto, CancellationToken ctk = default) => await Upd<ConfigurazioneTariffaDTO, Tariffa>(dto, ctk);
    }
}
