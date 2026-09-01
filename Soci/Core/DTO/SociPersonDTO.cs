using AppSystem;
using Models.Interfaces;
using Models.Tables;
using ViewModelServices.Core;

namespace Soci.Core.DTO
{
    public class SociPersonDTO : BaseDTO, IMap, IMappable<Person>
    {
        public SociPersonDTO() { }  

        public SociPersonDTO(Person table)
        {
            Id = table.Id;
            Cognome = table.SurName;
            Nome = table.FirstName;
            Natoil = table.Natoil;
            CodiceUnivoco = table.UniqueParam;

            var primoSocio = table.Soci.FirstOrDefault();

            CodiceSocio = primoSocio?.Id ?? 0;
            NumeroSocio = primoSocio?.NumeroSocio ?? "0";

            var primaTessera = primoSocio?.Tessere?.FirstOrDefault();

            CodiceTessera = primaTessera?.Id ?? 0;
            NumeroTessera = primaTessera?.NumeroTessera ?? string.Empty;
            Scadenza = primaTessera?.Scadenza ?? 0;
        }

        public Person ToTable()
        {
            return new Person
            {
                Id = Id,
                SurName = Cognome,
                FirstName = Nome,
                Natoil = Natoil,
                UniqueParam = CodiceUnivoco
            };
            
        }

        public void UpdateTable(Person existing)
        {
            if (existing == null) return;
            // Aggiorniamo solo i campi che possono cambiare
            existing.SurName = Cognome;
            existing.FirstName = Nome;
            existing.Natoil = Natoil;
            existing.UniqueParam = CodiceUnivoco;
            // Non tocchiamo l'ID!
        }

        public string Cognome { get; set; } = string.Empty;
        public int Natoil { get; set; }
        
        //public DateTime? NatoilDate { get; set; }
        public int CodiceSocio { get; set; }
        public string NumeroSocio { get; set; } = string.Empty;
        public int CodiceTessera { get; set; }
        public string NumeroTessera { get; set; } = string.Empty;
        public int Scadenza { get; set; }
        //public DateTime? ScadenzaDate { get; set; } = DateTime.Now;

        public string CodiceUnivoco { get; set; } = string.Empty;

        // 2. Aggiungi un controllo di sicurezza sulle date (se l'int è 0, ToShortDateString crasha)
        public override string Titolo => $"{Nome} {Cognome} ({Natoil.DateIntToDate().ToShortDateString()})";

        
    }

    


}
