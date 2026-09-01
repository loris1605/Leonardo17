using Models.Interfaces;
using Models.Tables;
using System.Linq.Expressions;
using ViewModelServices.Core;

namespace Configurazione.Core.DTO
{
    public class ConfigurazioneSettoreElencoDTO : BaseDTO, IMap
    {
        public string NomeSettore { get; set; } = string.Empty;
        public string EtichettaSettore { get; set; } = string.Empty;
        public int CodiceTipoSettore { get; set; } = 0;
        public string NomeTipoSettore { get; set; } = string.Empty;
        public bool HasReparto { get; set; }

        public override string Nome => NomeSettore;

        public ConfigurazioneSettoreElencoDTO() { }

        public static Expression<Func<Settore, ConfigurazioneSettoreElencoDTO>> ToSettoreElencoDto(int postazioneId)
        {
            return p => new ConfigurazioneSettoreElencoDTO
            {
                Id = p.Id,
                NomeSettore = p.Nome,
                EtichettaSettore = p.Label,
                CodiceTipoSettore = p.TipoSettoreId,
                NomeTipoSettore = p.TipoSettore!.Nome,
                // Usiamo il parametro del metodo (postazioneId) dentro l'espressione
                HasReparto = p.Reparti.Any(r => r.PostazioneId == postazioneId  )
            };
        }
    }
}
