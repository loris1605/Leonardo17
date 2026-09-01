using Cassa.Core.DTO;
using ReactiveUI;
using ViewModelServices.Core.Map;

namespace Cassa.ViewModels.Map
{
    public class EntraSocioMap : BindableMap
    {
        public EntraSocioMap() { }

        public EntraSocioMap(EntraSocioDTO dto)
        {
            this.NumeroTessera = dto.NumeroTessera;
            this.CodiceTessera = dto.CodiceTessera;
            this.Cognome = dto.Cognome;
            this.Nome = dto.Nome;
            this.Natoil = dto.Natoil;
            this.CodiceSocio = dto.CodiceSocio;
            this.NumeroSocio = dto.NumeroSocio;
            this.CodicePerson = dto.CodicePerson;
            this.Scadenza = dto.ScadenzaTessera;
            this.CodiceUnivoco = dto.CodiceUnivoco;
            this.Posizione = dto.Posizione;
            this.Consumazione = dto.Consumazione;
            this.Note = dto.Note;
        }

        public EntraSocioDTO ToDto()
        {
            return new EntraSocioDTO
            {
                NumeroTessera = this.NumeroTessera,
                CodiceTessera = this.CodiceTessera,
                Cognome = this.Cognome,
                Nome = this.Nome,
                Natoil = this.Natoil,
                CodicePerson = this.CodicePerson,
                CodiceSocio = this.CodiceSocio,
                NumeroSocio = this.NumeroSocio,
                ScadenzaTessera = this.Scadenza,
                CodiceUnivoco = this.CodiceUnivoco,
                Posizione = this.Posizione,
                Consumazione = this.Consumazione,
                Note = this.Note
            };
        }


        private string _numeroTessera = string.Empty;
        public string NumeroTessera
        {
            get => _numeroTessera;
            set => this.RaiseAndSetIfChanged(ref _numeroTessera, value);
        }

        private string _cognome = String.Empty;
        public string Cognome
        {
            get => _cognome;
            set => this.RaiseAndSetIfChanged(ref _cognome, value);
        }

        private string nome = string.Empty;
        public override string Nome
        {
            get => nome;
            set => this.RaiseAndSetIfChanged(ref nome, value);

        }

        private int _natoil;
        public int Natoil
        {
            get => _natoil;
            set => this.RaiseAndSetIfChanged(ref _natoil, value);

        }

        private int _codicesocio;
        public int CodiceSocio
        {
            get => _codicesocio;
            set => this.RaiseAndSetIfChanged(ref _codicesocio, value);

        }

        private string numerosocio = string.Empty;
        public string NumeroSocio
        {
            get => numerosocio;
            set => this.RaiseAndSetIfChanged(ref numerosocio, value);

        }

        private int _codicetessera;
        public int CodiceTessera
        {
            get => _codicetessera;
            set => this.RaiseAndSetIfChanged(ref _codicetessera, value);

        }


        private int _scadenza;
        public int Scadenza
        {
            get => _scadenza;
            set => this.RaiseAndSetIfChanged(ref _scadenza, value);

        }

        private string _codiceunivoco = string.Empty;
        public string CodiceUnivoco
        {
            get => _codiceunivoco;
            set => this.RaiseAndSetIfChanged(ref _codiceunivoco, value);

        }

        private string _posizione = string.Empty;
        public string Posizione
        {
            get => _posizione;
            set => this.RaiseAndSetIfChanged(ref _posizione, value);

        }

        private int _codiceperson;
        public int CodicePerson
        {
            get => _codiceperson;
            set => this.RaiseAndSetIfChanged(ref _codiceperson, value);
        }

        private decimal _consumazione;
        public decimal Consumazione
        {
            get => _consumazione;
            set => this.RaiseAndSetIfChanged(ref _consumazione, value);
        }

        private string _note = string.Empty;
        public string Note
        {
            get => _note;
            set => this.RaiseAndSetIfChanged(ref _note, value);
        }
    }
}
