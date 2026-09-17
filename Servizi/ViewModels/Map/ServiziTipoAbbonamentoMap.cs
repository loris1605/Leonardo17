using ReactiveUI;
using Servizi.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModelServices.Core.Map;

namespace Servizi.ViewModels.Map
{
    public class ServiziTipoAbbonamentoMap :BindableMap
    {
        public ServiziTipoAbbonamentoMap() { }
        public ServiziTipoAbbonamentoMap(ServiziTipoAbbonamentoDTO dto)
        {
            this.Id = dto.Id;
            this.Nome = dto.Nome;
            this.NumeroIngressi = dto.NumeroIngressi;
            this.DurataGiorni = dto.DurataGiorni;
            this.Prezzo = dto.Prezzo;
            this.Attivo = dto.Attivo;
        }
        public ServiziTipoAbbonamentoDTO ToDto()
        {
            return new ServiziTipoAbbonamentoDTO
            {
                Id = this.Id,
                Nome = this.Nome,
                NumeroIngressi = this.NumeroIngressi,
                DurataGiorni = this.DurataGiorni,
                Prezzo = this.Prezzo,
                Attivo = this.Attivo
            };
        }



        private string _nome = string.Empty;
        public string NomeAbbonamento
        {
            get => _nome;
            set => this.RaiseAndSetIfChanged(ref _nome, value);
        }

        private int _numeroIngressi;
        public int NumeroIngressi
        {
            get => _numeroIngressi;
            set => this.RaiseAndSetIfChanged(ref _numeroIngressi, value);
        }

        private int _DurataGiorni;
        public int DurataGiorni
        {
            get => _DurataGiorni;
            set => this.RaiseAndSetIfChanged(ref _DurataGiorni, value);
        }

        private decimal _Prezzo;
        public decimal Prezzo
        {
            get => _Prezzo;
            set => this.RaiseAndSetIfChanged(ref _Prezzo, value);
        }

        private bool _Attivo;
        public bool Attivo
        {
            get => _Attivo;
            set => this.RaiseAndSetIfChanged(ref _Attivo, value);
        }

    }
}
