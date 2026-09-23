using Models.Interfaces;
using Models.Tables;
using System.Linq.Expressions;
using ViewModelServices.Core;

namespace Servizi.Core.DTO
{
    public class ServiziTipoAbbonamentoDTO : BaseDTO, IMap, IMappable<TipoAbbonamento>
    {
        public ServiziTipoAbbonamentoDTO() { }

        public ServiziTipoAbbonamentoDTO(TipoAbbonamento table)
        {
            this.Id = table.Id;
            this.Nome = table.Nome;
            this.Prezzo = table.Prezzo;
            this.DurataGiorni = table.DurataGiorni;
            this.NumeroIngressi = table.NumeroIngressi;
            this.Attivo = table.Attivo;
        }

        public TipoAbbonamento ToTable()
        {
            return new TipoAbbonamento
            {
                Id = this.Id,
                Nome = this.Nome,
                Prezzo = this.Prezzo,
                DurataGiorni = this.DurataGiorni,
                NumeroIngressi = this.NumeroIngressi,
                Attivo = this.Attivo
            };
        }

        public void UpdateTable(TipoAbbonamento existing)
        {
            if (existing == null) return;
            existing.Nome = this.Nome;
            existing.Prezzo = this.Prezzo;
            existing.DurataGiorni = this.DurataGiorni;
            existing.NumeroIngressi = this.NumeroIngressi;
            existing.Attivo = this.Attivo;
        }

        public static Expression<Func<TipoAbbonamento, ServiziTipoAbbonamentoDTO>> ToTipoAbbonamentoDto => entity => new ServiziTipoAbbonamentoDTO
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Prezzo = entity.Prezzo,
            DurataGiorni = entity.DurataGiorni,
            NumeroIngressi = entity.NumeroIngressi,
            Attivo = entity.Attivo
        };





        public string Descrizione { get; set; } = string.Empty;
        public decimal Prezzo { get; set; }
        public int DurataGiorni { get; set; }
        public bool Attivo { get; set; }

        public int NumeroIngressi { get; set; }
    }
}
