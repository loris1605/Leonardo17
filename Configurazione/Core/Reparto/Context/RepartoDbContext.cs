using Microsoft.EntityFrameworkCore;
using Models.Context;
using Models.Tables;

namespace Configurazione.Core.Context
{
    public interface IRepartoDbContext
    {
        DbSet<Postazione> Postazioni { get; set; }
        DbSet<Reparto> Reparti { get; set; }
        DbSet<Settore> Settori { get; set; }
        DbSet<TipoSettore> TipiSettore { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public class RepartoDbContext : BaseContext, IRepartoDbContext
    {
        public DbSet<Reparto> Reparti { get; set; }
        public DbSet<Settore> Settori { get; set; }
        public DbSet<TipoSettore> TipiSettore { get; set; }
        public DbSet<Postazione> Postazioni { get; set; }


    }
}
