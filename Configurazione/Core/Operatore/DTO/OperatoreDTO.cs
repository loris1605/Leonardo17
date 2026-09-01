using Models.Interfaces;
using Models.Tables;
using System.Linq.Expressions;
using ViewModelServices.Core;

namespace Configurazione.Core.DTO
{
    public class ConfigurazioneOperatoreDTO : BaseDTO, IMap, IMappable<Operatore>
    {
        public ConfigurazioneOperatoreDTO() { }  

        public ConfigurazioneOperatoreDTO(Operatore table)
        {
            this.Id = table.Id;
            this.NomeOperatore = table.Nome;
            this.Password = table.Password;
            this.Abilitato = table.Abilitato;
        }

        public Operatore ToTable()
        {
            return new Operatore
            {
                Id = this.Id,
                Nome = this.NomeOperatore,
                Password = this.Password,
                Abilitato = this.Abilitato,
                Pass = this.Badge,
                PersonId = this.CodicePerson
            };
        }

        public void UpdateTable(Operatore existing)
        {
            if (existing == null) return;
            // Aggiorniamo solo i campi che possono cambiare
            existing.Nome = this.NomeOperatore;
            existing.Password = this.Password;
            existing.Abilitato = this.Abilitato;
            existing.Pass = this.Badge;
            // Non tocchiamo l'ID!
        }


        public static Expression<Func<Operatore, ConfigurazioneOperatoreDTO>> ToOperatoreDto => entity => new ConfigurazioneOperatoreDTO
        {
            Id = entity.Id,
            NomeOperatore = entity.Nome,
            Password = entity.Password,
            Abilitato = entity.Abilitato,
            Badge = entity.Pass
        };

        public static Expression<Func<Operatore, Permesso, ConfigurazioneOperatoreDTO>> ToOperatoriDtoRelationed =>
        (o, p) => new ConfigurazioneOperatoreDTO
        {
            Id = o.Id,
            NomeOperatore = o.Nome,
            Password = o.Password,
            Abilitato = o.Abilitato,
            Badge = o.Pass,
            CodicePerson = o.PersonId,
            CodicePermesso = p != null ? p.Id : 0,
            NomePostazione = p != null && p.Postazione != null ? p.Postazione.Nome : "Nessuna",
            TipoPostazione = p != null && p.Postazione != null && p.Postazione.TipoPostazione != null
                             ? p.Postazione.TipoPostazione.Nome
                             : "N/A"
        };

        public string NomeOperatore { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Abilitato { get; set; }
        public int Badge { get; set; }
        public int CodicePermesso { get; set; }
        public string NomePostazione { get; set; } = string.Empty;
        public string TipoPostazione { get; set; } = string.Empty;
        public int CodicePerson { get; set; }

        public override string Titolo => $"{NomeOperatore} - {(Abilitato ? "Abilitato" : "Non abilitato")}";

        
    }
}
