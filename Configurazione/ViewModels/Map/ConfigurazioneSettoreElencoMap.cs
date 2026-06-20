using Configurazione.Core.DTO;
using ReactiveUI;
using ViewModelServices.Core.Map;

namespace Configurazione.ViewModels.Map
{
    public class ConfigurazioneSettoreElencoMap : BindableMap
    {
        public ConfigurazioneSettoreElencoMap() { }

        public ConfigurazioneSettoreElencoMap(ConfigurazioneSettoreElencoDTO dto)
        {
            this.Id = dto.Id;
            this.NomeSettore = dto.NomeSettore;
            this.EtichettaSettore = dto.EtichettaSettore;
            this.CodiceTipoSettore = dto.CodiceTipoSettore;
            this.NomeTipoSettore = dto.NomeTipoSettore;
            this.HasReparto = dto.HasReparto;
        }

        public ConfigurazioneSettoreElencoDTO ToDto()
        {
            return new ConfigurazioneSettoreElencoDTO
            {
                Id = this.Id,
                NomeSettore = this.NomeSettore,
                EtichettaSettore = this.EtichettaSettore,
                CodiceTipoSettore = this.CodiceTipoSettore,
                NomeTipoSettore = this.NomeTipoSettore,
                HasReparto = this.HasReparto
            };
        }
        private string _nomesettore = string.Empty;
        public string NomeSettore
        {
            get => _nomesettore;
            set => this.RaiseAndSetIfChanged(ref _nomesettore, value);
        }
        private string _etichettasettore = string.Empty;
        public string EtichettaSettore
        {
            get => _etichettasettore;
            set => this.RaiseAndSetIfChanged(ref _etichettasettore, value);
        }
        private int _codicetiposettore;
        public int CodiceTipoSettore
        {
            get => _codicetiposettore;
            set => this.RaiseAndSetIfChanged(ref _codicetiposettore, value);
        }

        private string _nometipopostazione = string.Empty;
        public string NomeTipoSettore
        {
            get => _nometipopostazione;
            set => this.RaiseAndSetIfChanged(ref _nometipopostazione, value);
        }

        private bool _haspermesso;
        public bool HasReparto
        {
            get => _haspermesso;
            set => this.RaiseAndSetIfChanged(ref _haspermesso, value);
        }
    }
}
