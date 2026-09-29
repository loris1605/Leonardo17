namespace Models.Tables
{
    public class Abbonamento : IStandardTable
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int TipoAbbonamentoId { get; set; }
        public int PersonId1 { get; set; }
        public int PersonId2 { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
        public bool Attivo { get; set; }
        public string Note { get; set; } = string.Empty;

        public Person? Person1 { get; set; }
        public Person? Person2 { get; set; }
        public TipoAbbonamento? TipoAbbonamento { get; set; }

        public List<AbbonamentoConto> AbbonamentoConti { get; set; } = new List<AbbonamentoConto>();
    }
    
}
