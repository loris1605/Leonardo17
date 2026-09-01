using Cassa.Core.DTO;
using ReactiveUI;
using ViewModelServices.Core.Map;

namespace Cassa.ViewModels.Map
{
    public class EntraIngressiMap : BindableMap
    {
        public EntraIngressiMap() { }

        public EntraIngressiMap(EntraIngressiDTO dto)
        {
            this.Id = dto.Id;
            this.NomeTariffa = dto.NomeTariffa;
            this.EtichettaTariffa = dto.EtichettaTariffa;
            this.PrezzoTariffa = dto.PrezzoTariffa;
            this.IsFreeDrink = dto.IsFreeDrink;
        }

<<<<<<< HEAD
        public EntraIngressiDTO ToDto()
=======
        public EntraIngressiDTO ToDTO()
>>>>>>> c42a9bd06f481eca069f8d12d0e299d3147dafa4
        {
            return new EntraIngressiDTO
            {
                Id = this.Id,
                NomeTariffa = this.NomeTariffa,
                EtichettaTariffa = this.EtichettaTariffa,
                PrezzoTariffa = this.PrezzoTariffa,
                IsFreeDrink = this.IsFreeDrink
            };
<<<<<<< HEAD
        }
=======
        }   

>>>>>>> c42a9bd06f481eca069f8d12d0e299d3147dafa4

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

        private bool _isfreedrink;
        public bool IsFreeDrink
        {
            get => _isfreedrink;
            set => this.RaiseAndSetIfChanged(ref _isfreedrink, value);
        }

    }
}
