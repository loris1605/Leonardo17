using Models.Interfaces;
using Models.Tables;
using System.Linq.Expressions;
using ViewModelServices.Core;

namespace Configurazione.Core.DTO
{
    public class ConfigurazioneTipoSettoreDTO : BaseDTO, IMap, IMappable<TipoSettore>
    {
        public ConfigurazioneTipoSettoreDTO() { }
        public ConfigurazioneTipoSettoreDTO(TipoSettore table)
        {
            this.Id = table.Id;
            this.Nome = table.Nome;
        }
        public TipoSettore ToTable()
        {
            return new TipoSettore
            {
                Id = this.Id,
                Nome = this.Nome
            };
        }
        public void UpdateTable(TipoSettore existing)
        {
            if (existing == null) return;
            existing.Nome = this.Nome;
        }

        public static Expression<Func<TipoSettore, ConfigurazioneTipoSettoreDTO>> ToDto => t => new ConfigurazioneTipoSettoreDTO
        {
            Id = t.Id,
            Nome = t.Nome
        };
    }
}
