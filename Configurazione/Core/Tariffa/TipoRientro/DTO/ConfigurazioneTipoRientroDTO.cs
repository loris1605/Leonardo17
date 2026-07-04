using Models.Interfaces;
using Models.Tables;
using System.Linq.Expressions;
using ViewModelServices.Core;

namespace Configurazione.Core.DTO
{
    public class ConfigurazioneTipoRientroDTO : BaseDTO, IMap, IMappable<TipoRientro>
    {
        public ConfigurazioneTipoRientroDTO() { }

        public ConfigurazioneTipoRientroDTO(TipoRientro table)
        {
            this.Id = table.Id;
            this.Nome = table.Nome;
            this.DurataOre = table.DurataOre;
            
        }

        public int DurataOre { get; set; }
        public bool HasPostazione { get; set; }

        public TipoRientro ToTable()
        {
            return new TipoRientro
            {
                Id = this.Id,
                Nome = this.Nome,
                DurataOre = this.DurataOre
            };
        }

        public void UpdateTable(TipoRientro existing)
        {
            if (existing == null) return;
            existing.Nome = this.Nome;
            existing.DurataOre = this.DurataOre;
        }

        public static Expression<Func<TipoRientro, ConfigurazioneTipoRientroDTO>> ToDto => t => new ConfigurazioneTipoRientroDTO
        {
            Id = t.Id,
            Nome = t.Nome,
            DurataOre = t.DurataOre,
            HasPostazione = t.Postazioni.Any() // Assuming Postazioni is a collection in TipoRientro
        };
    }
}
