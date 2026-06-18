using Configurazione.Core.Context;
using Configurazione.Core.DTO;
using Microsoft.EntityFrameworkCore;
using Models.Repository;
using Models.Tables;
using System.Linq.Expressions;

namespace Configurazione.Core.Repository
{
    public interface IConfigurazioneOperatoreRepository : IBaseRepository<Operatore>
    {
        Task<ConfigurazioneOperatoreDTO> FirstOperatore(int id, CancellationToken ctk = default);
        Task<List<ConfigurazioneOperatoreDTO>> Load(int id, CancellationToken ctk = default);
        Task<List<ConfigurazioneOperatoreDTO>> LoadByModel(object model, CancellationToken ctk = default);
        Task<List<ConfigurazioneOperatoreDTO>> LoadOperatori(Expression<Func<Operatore, bool>> predicate, CancellationToken ctk = default);
        Task<bool> Upd(ConfigurazioneOperatoreDTO dto, CancellationToken ctk = default);
        
    }

    public class ConfigurazioneOperatoreRepository(IOperatoreDbContext ctx) : 
                    BaseRepository<OperatoreDbContext, Operatore>, IConfigurazioneOperatoreRepository
    {
        private readonly IOperatoreDbContext _ctx = ctx;

        public async Task<List<ConfigurazioneOperatoreDTO>> Load(int id, CancellationToken ctk = default)
        {
            if (id > 0)
                return await LoadOperatori(x => x.Id == id, ctk);
            else
                return await LoadOperatori(p => p.Id > -2, ctk);
        }

        public async Task<List<ConfigurazioneOperatoreDTO>> LoadOperatori(Expression<Func<Operatore, bool>> predicate
                                                            , CancellationToken ctk = default)
        {
            
            // Carichiamo prima gli operatori con i loro dati (Eager Loading)
            var data = await _ctx.Operatori
                .AsNoTracking()
                .Where(predicate)
                .OrderBy(o => o.Nome)
                .SelectMany(o => o.Permessi.DefaultIfEmpty(), ConfigurazioneOperatoreDTO.ToOperatoriDtoRelationed) // <--- Usi l'espressione qui
                .ToListAsync(ctk);

            return data;

        }

        public async Task<bool> Upd(ConfigurazioneOperatoreDTO dto, CancellationToken ctk = default) => 
            await Upd<ConfigurazioneOperatoreDTO, Operatore>(dto, ctk);

        public async Task<ConfigurazioneOperatoreDTO> FirstOperatore(int id, CancellationToken ctk = default)
        {

            var result = await GetById(id,
                selector: ConfigurazioneOperatoreDTO.ToOperatoreDto, ctk);
                

            return result ?? new ConfigurazioneOperatoreDTO();
        }


        public async Task<List<ConfigurazioneOperatoreDTO>> LoadByModel(object model, CancellationToken ctk = default)
        {

            await Task.FromResult(new List<ConfigurazioneOperatoreDTO>());
            throw new NotImplementedException();
        }

        
    }
}
