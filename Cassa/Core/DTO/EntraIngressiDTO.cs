using Models.Interfaces;
using ViewModelServices.Core;

namespace Cassa.Core.DTO
{
    public class EntraIngressiDTO : BaseDTO, IMap
    {
        public string NomeTariffa { get; set; } = string.Empty;
        public string EtichettaTariffa { get; set; } = string.Empty;
        public decimal PrezzoTariffa { get; set; } = decimal.Zero;
        public bool IsFreeDrink { get; set; }
        
    }
}
