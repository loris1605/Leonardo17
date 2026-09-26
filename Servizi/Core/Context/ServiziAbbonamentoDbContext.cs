using Microsoft.EntityFrameworkCore;
using Models.Context;
using Models.Tables;

namespace Servizi.Core.Context
{
    public interface IServiziAbbonamentoDbContext
    {
        DbSet<TipoAbbonamento> TipiAbbonamento { get; set; }
        DbSet<Postazione> Postazioni { get; set; }
    }

    public class ServiziAbbonamentoDbContext : BaseContext, IServiziAbbonamentoDbContext
    {
        public DbSet<TipoAbbonamento> TipiAbbonamento { get; set; } = null!;
        public DbSet<Postazione> Postazioni { get; set; } = null!;
    }
}
