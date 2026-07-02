using Configurazione.Core.Context;
using Configurazione.Core.DTO;
using Microsoft.EntityFrameworkCore;
using Models.Repository;
using Models.Tables;
using System.Linq.Expressions;

namespace Configurazione.Core.Repository
{
    public interface ITipoRientroRepository
    {
        Task<TipoRientroDTO> FirstTipoRientro(int id);
        Task<List<TipoRientroDTO>> Load(int id, CancellationToken ctk = default);
        Task<bool> Upd(TipoRientroDTO dto, CancellationToken ctk = default);
    }

    public class TipoRientroRepository(ITipoRientroDbContext ctx) : BaseRepository<TipoRientroDbContext, TipoRientro>, ITipoRientroRepository
    {
        private readonly ITipoRientroDbContext _ctx = ctx;

        public async Task<List<TipoRientroDTO>> Load(int id, CancellationToken ctk = default)
        {
            if (id > 0)
                return await LoadTipoRientri(x => x.Id == id, ctk);
            else
                return await LoadTipoRientri(p => p.Id > 0, ctk);
        }

        private async Task<List<TipoRientroDTO>> LoadTipoRientri(Expression<Func<TipoRientro, bool>> predicate
                                                            , CancellationToken ctk = default)
        {

            // Carichiamo prima gli operatori con i loro dati (Eager Loading)
            return await _ctx.TipiRientro
                .AsNoTracking()
                .Where(predicate)
                .OrderBy(o => o.Nome)
                .Select(TipoRientroDTO.ToDto) // <--- Usi l'espressione qui
                .ToListAsync(ctk);

        }

        public async Task<TipoRientroDTO> FirstTipoRientro(int id) =>
            await GetById(id, selector: TipoRientroDTO.ToDto) ?? new TipoRientroDTO();

        public async Task<bool> Upd(TipoRientroDTO dto, CancellationToken ctk = default) =>
            await Upd<TipoRientroDTO, TipoRientro>(dto, ctk);
    }
}
