using Configurazione.Core.DTO;
using ReactiveUI;
using ViewModelServices.Core.Map;

namespace Configurazione.ViewModels.Map
{
    public class ConfigurazioneTipoRientroMap : BindableMap
    {
        public ConfigurazioneTipoRientroMap() { }

        public ConfigurazioneTipoRientroMap(ConfigurazioneTipoRientroDTO dto)
        {
            this.Id = dto.Id;
            this.Nome = dto.Nome;
            this.DurataOre = dto.DurataOre;
        }

        public ConfigurazioneTipoRientroDTO ToDto()
        {
            return new ConfigurazioneTipoRientroDTO
            {
                Id = this.Id,
                Nome = this.Nome,
                DurataOre = this.DurataOre

            };
        }

        private string _nomepostazione = string.Empty;
        public override string Nome
        {
            get => _nomepostazione;
            set => this.RaiseAndSetIfChanged(ref _nomepostazione, value);
        }

        private int _mydurataore;
        public int DurataOre
        {
            get => _mydurataore;
            set => this.RaiseAndSetIfChanged(ref _mydurataore, value);
        }
    }
}
