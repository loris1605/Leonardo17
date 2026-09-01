using Configurazione.Core.DTO;
using ReactiveUI;
using ViewModelServices.Core.Map;

namespace Configurazione.ViewModels.Map
{
    public class ConfigurazionePostazioneElencoMap : BindableMap
    {
        public ConfigurazionePostazioneElencoMap() { }

        public ConfigurazionePostazioneElencoMap(ConfigurazionePostazioneElencoDTO dto)
        {
            this.Id = dto.Id;
            this.CodiceTipoPostazione = dto.CodiceTipoPostazione;
            this.NomePostazione = dto.NomePostazione;
            this.NomeTipoPostazione = dto.NomeTipoPostazione;
            this.HasPermesso = dto.HasPermesso;
        }

        public ConfigurazionePostazioneElencoDTO ToDto()
        {
            return new ConfigurazionePostazioneElencoDTO
            {
                Id = this.Id,
                CodiceTipoPostazione = this.CodiceTipoPostazione,
                NomePostazione = this.NomePostazione,
                NomeTipoPostazione = this.NomeTipoPostazione,
                HasPermesso = this.HasPermesso,
            };
        }


        private int _codicetipopostazione;
        public int CodiceTipoPostazione
        {
            get => _codicetipopostazione;
            set => this.RaiseAndSetIfChanged(ref _codicetipopostazione, value);
        }

        private string _nomepostazione = string.Empty;
        public string NomePostazione
        {
            get => _nomepostazione;
            set => this.RaiseAndSetIfChanged(ref _nomepostazione, value);
        }

        private string _nometipopostazione = string.Empty;
        public string NomeTipoPostazione
        {
            get => _nometipopostazione;
            set => this.RaiseAndSetIfChanged(ref _nometipopostazione, value);
        }

        private bool _haspermesso;
        public bool HasPermesso
        {
            get => _haspermesso;
            set => this.RaiseAndSetIfChanged(ref _haspermesso, value);
        }
    }
}
