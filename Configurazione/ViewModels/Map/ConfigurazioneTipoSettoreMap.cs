using Configurazione.Core.DTO;
using ReactiveUI;
using ViewModelServices.Core.Map;

namespace Configurazione.ViewModels.Map
{
    public class ConfigurazioneTipoSettoreMap : BindableMap
    {
        public ConfigurazioneTipoSettoreMap() { }

        private string _nometiposettore = string.Empty;
        public override string Nome
        {
            get => _nometiposettore;
            set => this.RaiseAndSetIfChanged(ref _nometiposettore, value);
        }
         public ConfigurazioneTipoSettoreMap(ConfigurazioneTipoSettoreDTO dto)
        {
            this.Id = dto.Id;
            this.Nome = dto.Nome;
        }

        public ConfigurazioneTipoSettoreDTO ToDto()
        {
            return new ConfigurazioneTipoSettoreDTO
            {
                Id = this.Id,
                Nome = this.Nome
            };
        }

        
    }
}
