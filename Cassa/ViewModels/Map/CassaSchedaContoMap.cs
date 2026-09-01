using Cassa.Core.DTO;
using ReactiveUI;
using ViewModelServices.Core.Map;

namespace Cassa.ViewModels.Map
{
    public class CassaSchedaContoMap : BindableMap
    {
        public CassaSchedaContoMap() { }

        public CassaSchedaContoMap(CassaSchedaContoDTO dto)
        {
            this.Id = dto.Id;
            this.CodiceScheda = dto.CodiceScheda;
            this.DescSettore = dto.DescSettore;
            this.DescPostazione = dto.DescPostazione;
            this.VoiceDesc = dto.VoiceDesc;
            this.VoicePrice = dto.VoicePrice;
            this.Pagato = dto.Pagato;
            this.Note = dto.Note;
            this.DataOra = dto.DataOra;
        }

        public CassaSchedaContoDTO ToDto()
        {
            return new CassaSchedaContoDTO
            {
                Id = this.Id,
                CodiceScheda = this.CodiceScheda,
                DescSettore = this.DescSettore,
                DescPostazione = this.DescPostazione,
                VoiceDesc = this.VoiceDesc,
                VoicePrice = this.VoicePrice,
                Pagato = this.Pagato,
                Note = this.Note,
                DataOra = this.DataOra
            };
        }



        private int _codiceScheda;
        public int CodiceScheda
        {
            get => _codiceScheda;
            set => this.RaiseAndSetIfChanged(ref _codiceScheda, value);
        }

        private string _descSettore = string.Empty;
        public string DescSettore
        {
            get => _descSettore;
            set => this.RaiseAndSetIfChanged(ref _descSettore, value);
        }

        private string _descPostazione = string.Empty;
        public string DescPostazione
        {
            get => _descPostazione;
            set => this.RaiseAndSetIfChanged(ref _descPostazione, value);
        }

        private string _voiceDesc = string.Empty;
        public string VoiceDesc
        {
            get => _voiceDesc;
            set => this.RaiseAndSetIfChanged(ref _voiceDesc, value);
        }

        private decimal _voicePrice;
        public decimal VoicePrice
        {
            get => _voicePrice;
            set => this.RaiseAndSetIfChanged(ref _voicePrice, value);
        }

        private bool _pagato;
        public bool Pagato
        {
            get => _pagato;
            set => this.RaiseAndSetIfChanged(ref _pagato, value);
        }

        private string _note = string.Empty;
        public string Note
        {
            get => _note;
            set => this.RaiseAndSetIfChanged(ref _note, value);
        }

        private DateTime _dataOra;
        public DateTime DataOra
        {
            get => _dataOra;
            set => this.RaiseAndSetIfChanged(ref _dataOra, value);

        }
    }
}
