using Models.Interfaces;
using Models.Tables;
using System.Linq.Expressions;
using ViewModelServices.Core;

namespace Configurazione.Core.DTO
{
    public class ConfigurazioneTariffaDTO : BaseDTO, IMap, IMappable<Tariffa>
    {
        public ConfigurazioneTariffaDTO() { }

        public string NomeTariffa { get; set; } = string.Empty;
        public string EtichettaTariffa { get; set; } = string.Empty;
        public decimal PrezzoTariffa { get; set; } = decimal.Zero;
        public bool IsFreeDrink { get; set; }
        public bool HasListino { get; set; }

        // Sincronizzazione Getter/Setter per l'interfaccia IMap
        public override string Nome
        {
            get => NomeTariffa;
            set => NomeTariffa = value ?? string.Empty;
        }

        // Titolo descrittivo per ComboBox o liste: Nome + Prezzo
        public override string Titolo => $"{NomeTariffa} - " +
            $"{PrezzoTariffa:C2} {(IsFreeDrink ? "(Free Drink)" : "")}";



        public ConfigurazioneTariffaDTO(Tariffa table)
        {
            this.Id = table.Id;
            this.NomeTariffa = table.Nome;
            this.EtichettaTariffa = table.Label;
            this.PrezzoTariffa = table.Prezzo;
            this.IsFreeDrink = table.IsFreeDrink;
        }

        public Tariffa ToTable()
        {
            return new Tariffa
            {
                Id = this.Id,
                Nome = this.NomeTariffa,
                Label = this.EtichettaTariffa,
                Prezzo = this.PrezzoTariffa,
                IsFreeDrink = this.IsFreeDrink
            };
        }

        public void UpdateTable(Tariffa existing)
        {
            if (existing == null) return;
            // Aggiorniamo solo i campi che possono cambiare
            existing.Nome = this.NomeTariffa;
            existing.Label = this.EtichettaTariffa;
            existing.Prezzo = this.PrezzoTariffa;
            existing.IsFreeDrink = this.IsFreeDrink;
            // Non tocchiamo l'ID!
        }

        public static Expression<Func<Tariffa, ConfigurazioneTariffaDTO>> ToDto => entity => new ConfigurazioneTariffaDTO
        {
            Id = entity.Id,
            NomeTariffa = entity.Nome,
            EtichettaTariffa = entity.Label,
            PrezzoTariffa = entity.Prezzo,
            IsFreeDrink = entity.IsFreeDrink,
            HasListino = entity.Listini.Any() // Se la tariffa è associata a qualche listino, HasListino sarà true
        };

        public static Expression<Func<Tariffa, ConfigurazioneTariffaDTO>> ToTariffaElencoDto(int settoreId)
        {
            return p => new ConfigurazioneTariffaDTO
            {
                Id = p.Id,
                NomeTariffa = p.Nome,
                EtichettaTariffa = p.Label,
                PrezzoTariffa = p.Prezzo,
                IsFreeDrink = p.IsFreeDrink,
                HasListino = p.Listini.Any(l => l.SettoreId == settoreId)
            };
                
        }
    }
}