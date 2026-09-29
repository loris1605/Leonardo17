namespace Models.Tables
{
    public class AbbonamentoConto : IStandardTable
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int AbbonamentoId { get; set; }
        public int NumeroIngresso { get; set; }
        public DateTime DataIngresso { get; set; }
        public bool Entrato { get; set; }

        public Abbonamento? Abbonamento { get; set; }
    }
}
