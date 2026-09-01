using Configurazione.Core.DTO;
using ReactiveUI;
using ViewModelServices.Core.Map;

namespace Configurazione.ViewModels.Map
{
    public class ConfigurazioneTipoPostazioneMap : BindableMap
    {
        public ConfigurazioneTipoPostazioneMap() { }

        private string _nomepostazione = string.Empty;
        public override string Nome
        {
            get => _nomepostazione;
            set => this.RaiseAndSetIfChanged(ref _nomepostazione, value);
        }

        public ConfigurazioneTipoPostazioneMap(ConfigurazioneTipoPostazioneDTO dto)
        {
            this.Id = dto.Id;
            this.Nome = dto.Nome;
            
        }

        public ConfigurazioneTipoPostazioneDTO ToDto()
        {
            return new ConfigurazioneTipoPostazioneDTO
            {
                Id = this.Id,
                Nome = this.Nome
               
            };
        }
    }
}
