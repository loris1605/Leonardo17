using Configurazione.Core.DTO;
using ReactiveUI;
using ViewModelServices.Core.Map;

namespace Configurazione.ViewModels.Map
{
    public class ConfigurazioneSettoreMap : BindableMap
    {
        public override string Nome => NomeSettore;

        public ConfigurazioneSettoreMap() { }       
        public ConfigurazioneSettoreMap(ConfigurazioneSettoreDTO dto)
        {
            this.Id = dto.Id;
            this.NomeSettore = dto.NomeSettore;
            this.EtichettaSettore = dto.EtichettaSettore;
            this.CodiceTipoSettore = dto.CodiceTipoSettore;
            this.CodiceListino = dto.CodiceListino;
            this.NomeTariffa = dto.NomeTariffa;
            this.EtichettaTariffa = dto.EtichettaTariffa;
            this.PrezzoTariffa = dto.PrezzoTariffa;
            this.HasReparto = dto.HasReparto;
        }

        public ConfigurazioneSettoreDTO ToDto()
        {
            return new ConfigurazioneSettoreDTO
            {
                Id = this.Id,
                NomeSettore = this.NomeSettore,
                EtichettaSettore = this.EtichettaSettore,
                CodiceTipoSettore = this.CodiceTipoSettore,
                CodiceListino = this.CodiceListino,
                NomeTariffa = this.NomeTariffa,
                EtichettaTariffa = this.EtichettaTariffa,
                PrezzoTariffa = this.PrezzoTariffa,
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

        private string _nometiposettore = string.Empty;
        public string NomeTipoSettore
        {
            get => _nometiposettore;
            set => this.RaiseAndSetIfChanged(ref _nometiposettore, value);
        }

        private int _codicereparto;
        public int CodiceListino
        {
            get => _codicereparto;
            set => this.RaiseAndSetIfChanged(ref _codicereparto, value);
        }

        private string _nometariffa = string.Empty;
        public string NomeTariffa
        {
            get => _nometariffa;
            set => this.RaiseAndSetIfChanged(ref _nometariffa, value);
        }

        private string _etichettatariffa = string.Empty;
        public string EtichettaTariffa
        {
            get => _etichettatariffa;
            set => this.RaiseAndSetIfChanged(ref _etichettatariffa, value);
        }

        private decimal _prezzotariffa = 0M;
        public decimal PrezzoTariffa
        {
            get => _prezzotariffa;
            set => this.RaiseAndSetIfChanged(ref _prezzotariffa, value);
        }

        private bool _hasreparto;
        public bool HasReparto
        {
            get => _hasreparto;
            set => this.RaiseAndSetIfChanged(ref _hasreparto, value);
        }

        public override string Titolo => $"{NomeSettore} - {NomeTipoSettore}";
    }
}
