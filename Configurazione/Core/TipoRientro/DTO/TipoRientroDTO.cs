using Models.Interfaces;
using Models.Tables;
using System.Linq.Expressions;
using ViewModelServices.Core;

namespace Configurazione.Core.DTO
{
    public class TipoRientroDTO : BaseDTO, IMap, IMappable<TipoRientro>
    {
        public TipoRientroDTO() { }

        public string NomeTipoRientro { get; set; } = string.Empty;
        public int DurataOre { get; set; } = 0;

        public override string Nome
        {
            get => NomeTipoRientro;
            set => NomeTipoRientro = value ?? string.Empty;
        }

        public override string Titolo => $"{NomeTipoRientro} - " + $"{DurataOre} ore";

        public TipoRientroDTO(TipoRientro table)
        {
            this.Id = table.Id;
            this.NomeTipoRientro = table.Nome;
            this.DurataOre = table.DurataOre;
            
        }


        public TipoRientro ToTable()
        {
            return new TipoRientro
            {
                Id = this.Id,
                Nome = this.NomeTipoRientro,
                DurataOre = this.DurataOre
            };
        }

        public void UpdateTable(TipoRientro existing)
        {
            if (existing == null) return;
            existing.Nome = this.NomeTipoRientro;
            existing.DurataOre = this.DurataOre;
        }

        public static Expression<Func<TipoRientro, TipoRientroDTO>> ToDto => entity => new TipoRientroDTO
        {
            Id = entity.Id,
            NomeTipoRientro = entity.Nome,
            DurataOre = entity.DurataOre
        };
    }
}
