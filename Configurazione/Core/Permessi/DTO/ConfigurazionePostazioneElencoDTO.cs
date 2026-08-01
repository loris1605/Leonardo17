using Models.Interfaces;
using Models.Tables;
using System.Linq.Expressions;
using ViewModelServices.Core;

namespace Configurazione.Core.DTO
{
    public class ConfigurazionePostazioneElencoDTO : BaseDTO, IMap
    {
        public int CodiceTipoPostazione { get; set; }
        public string NomePostazione { get; set; } = string.Empty;
        public string NomeTipoPostazione { get; set; } = string.Empty;
        public bool HasPermesso { get; set; }

        public override string Nome
        {
            get => NomePostazione;
            set => NomePostazione = value ?? string.Empty;
        }

        public ConfigurazionePostazioneElencoDTO() { }

        public static Expression<Func<Postazione, ConfigurazionePostazioneElencoDTO>> ToPostazioneElencoDto(int operatoreId)
        {
            return p => new ConfigurazionePostazioneElencoDTO
            {
                Id = p.Id,
                Nome = p.Nome,
                NomeTipoPostazione = p.TipoPostazione!.Nome,
                // Usiamo il parametro del metodo (operatoreId) dentro l'espressione
                HasPermesso = p.Permessi.Any(perm => perm.OperatoreId == operatoreId)
            };
        }
    }
}
