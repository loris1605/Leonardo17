using Microsoft.EntityFrameworkCore;
using Models.Repository;
using Models.Tables;
using Servizi.Core.Context;
using Servizi.Core.DTO;
using System.Linq.Expressions;

namespace Servizi.Core.Repository
{
    public interface IServiziAbbonamentoRepository : IBaseRepository<TipoAbbonamento>
    {
        Task<ServiziTipoAbbonamentoDTO> FirstAbbonamento(int id, CancellationToken ctk = default);
        Task<List<ServiziTipoAbbonamentoDTO>> Load(int id, CancellationToken ctk = default);
        Task<List<ServiziTipoAbbonamentoDTO>> LoadAbbonamenti(Expression<Func<TipoAbbonamento, bool>> predicate, CancellationToken ctk = default);
        Task<bool> Upd(ServiziTipoAbbonamentoDTO dto, CancellationToken ctk = default);
    }

    public class ServiziAbbonamentoRepository(IServiziAbbonamentoDbContext ctx) :
                    BaseRepository<ServiziAbbonamentoDbContext, TipoAbbonamento>, IServiziAbbonamentoRepository
    {

        private readonly IServiziAbbonamentoDbContext _ctx = ctx;

        public async Task<List<ServiziTipoAbbonamentoDTO>> Load(int id, CancellationToken ctk = default)
        {
            if (id > 0)
                return await LoadAbbonamenti(x => x.Id == id, ctk);
            else
                return await LoadAbbonamenti(p => p.Id > 0, ctk);
        }

        public async Task<List<ServiziTipoAbbonamentoDTO>> LoadAbbonamenti(Expression<Func<TipoAbbonamento, bool>> predicate
                                                            , CancellationToken ctk = default)
        {
            // Carichiamo prima gli abbonamenti con i loro dati (Eager Loading)
            var data = await _ctx.TipiAbbonamento
                .AsNoTracking()
                .Where(predicate)
                .OrderBy(o => o.Nome)
                .Select(ServiziTipoAbbonamentoDTO.ToTipoAbbonamentoDto) // <--- Usi l'espressione qui
                .ToListAsync(ctk);
            return data;
        }

        public async Task<bool> Upd(ServiziTipoAbbonamentoDTO dto, CancellationToken ctk = default) =>
            await Upd<ServiziTipoAbbonamentoDTO, TipoAbbonamento>(dto, ctk);

        public async Task<ServiziTipoAbbonamentoDTO> FirstAbbonamento(int id, CancellationToken ctk = default)
        {
            var result = await GetById(id,
                selector: ServiziTipoAbbonamentoDTO.ToTipoAbbonamentoDto, ctk);
            return result ?? new ServiziTipoAbbonamentoDTO();
        }


    }
}
