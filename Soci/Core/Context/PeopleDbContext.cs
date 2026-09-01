using Microsoft.EntityFrameworkCore;
using Models.Context;
using Models.Tables;

namespace Soci.Core.Context
{
    public interface IPeopleDbContext
    {
        DbSet<Person> People { get; set; }
        DbSet<Socio> Soci { get; set; }
        DbSet<Tessera> Tessere { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public class PeopleDbContext : BaseContext, IPeopleDbContext
    {
        public DbSet<Person> People { get; set; } = null!;
        public DbSet<Socio> Soci { get; set; } = null!;
        public DbSet<Tessera> Tessere { get; set; } = null!;
    }
}
