using Microsoft.EntityFrameworkCore;
using Models.Context;
using Models.Tables;

namespace Configurazione.Core.Context
{
    public interface ITariffaDbContext
    {
        DbSet<Listino> Listini { get; set; }
        DbSet<Tariffa> Tariffe { get; set; }
    }

    public class TariffaDbContext : BaseContext, ITariffaDbContext
    {
        public DbSet<Tariffa> Tariffe { get; set; } = null!;
        public DbSet<Listino> Listini { get; set; } = null!;
    }
}
