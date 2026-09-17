using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Tables
{
    public class TipoAbbonamento : IStandardTable
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int NumeroIngressi { get; set; }
        public int DurataGiorni { get; set; }
        public decimal Prezzo { get; set; }
        public bool Attivo { get; set; }

    }
    
}
