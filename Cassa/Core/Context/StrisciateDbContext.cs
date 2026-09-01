using Microsoft.EntityFrameworkCore;
using Models.Context;
using Models.Tables;

namespace Cassa.Core.Context
{
    public interface IStrisciateDbContext
    {
        DbSet<Person> People { get; set; }
        DbSet<Socio> Soci { get; set; }
        DbSet<Strisciata> Strisciate { get; set; }
        DbSet<Tessera> Tessere { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public class StrisciateDbContext : BaseContext, IStrisciateDbContext
    {
        public DbSet<Strisciata> Strisciate { get; set; }
        public DbSet<Person> People { get; set; } = null!;
        public DbSet<Socio> Soci { get; set; } = null!;
        public DbSet<Tessera> Tessere { get; set; } = null!;
    }
}
