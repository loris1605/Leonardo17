using Models.Interfaces;
using Models.Tables;
using System.Linq.Expressions;
using ViewModelServices.Core;

namespace Configurazione.Core.DTO
{
    public class ConfigurazioneTipoPostazioneDTO : BaseDTO, IMap, IMappable<TipoPostazione>
    {
        public ConfigurazioneTipoPostazioneDTO() { }

        public ConfigurazioneTipoPostazioneDTO(TipoPostazione table)
        {
            this.Id = table.Id;
            this.Nome = table.Nome;
        }

        public TipoPostazione ToTable()
        {
            return new TipoPostazione
            {
                Id = this.Id,
                Nome = this.Nome
            };
        }

        public void UpdateTable(TipoPostazione existing)
        {
            if (existing == null) return;
            existing.Nome = this.Nome;
        }

        public static Expression<Func<TipoPostazione, ConfigurazioneTipoPostazioneDTO>> ToDto => t => 
                    new ConfigurazioneTipoPostazioneDTO
        {
            Id = t.Id,
            Nome = t.Nome
        };
    }
}
