using Models.Interfaces;
using Models.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModelServices.Core;

namespace Cassa.Core.DTO 
{
    public class CassaSchedaContoDTO : BaseDTO, IMap
    {
        public int CodiceScheda { get; set; }
        public string DescSettore { get; set; } = string.Empty;
        public string DescPostazione { get; set; } = string.Empty;
        public string VoiceDesc { get; set; } = string.Empty;
        public decimal VoicePrice { get; set; }
        public bool Pagato { get; set; }
        public string Note { get; set; } = string.Empty;
        public DateTime DataOra { get; set; }

        public CassaSchedaContoDTO() { }

        public CassaSchedaContoDTO(SchedaConto table)
        {
            this.Id = table.Id;
            this.CodiceScheda = table.SchedaId;
            this.DescSettore = table.DescSettore;
            this.DescPostazione = table.DescPostazione;
            this.VoiceDesc = table.VoiceDesc;
            this.VoicePrice = table.VoicePrice;
            this.Pagato = table.Pagato;
            this.Note = table.Note;
            this.DataOra = table.DataOra;
        }

        public SchedaConto ToTable()
        {
            return new SchedaConto
            {
                Id = this.Id,
                SchedaId = this.CodiceScheda,
                DescSettore = this.DescSettore,
                DescPostazione = this.DescPostazione,
                VoiceDesc = this.VoiceDesc,
                VoicePrice = this.VoicePrice,
                Pagato = this.Pagato,
                Note = this.Note,
                DataOra = this.DataOra
            };
        }

        public void UpdateTable(SchedaConto existing)
        {
            if (existing == null) return;

            existing.SchedaId = this.CodiceScheda;
            existing.DescSettore = this.DescSettore;
            existing.DescPostazione = this.DescPostazione;
            existing.VoiceDesc = this.VoiceDesc;
            existing.VoicePrice = this.VoicePrice;
            existing.Pagato = this.Pagato;
            existing.Note = this.Note;
            existing.DataOra = this.DataOra;
        }
    }
}
