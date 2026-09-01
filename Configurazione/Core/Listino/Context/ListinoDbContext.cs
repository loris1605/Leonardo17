using Microsoft.EntityFrameworkCore;
using Models.Context;
using Models.Tables;

namespace Configurazione.Core.Context
{
    public interface IListinoDbContext
    {
        DbSet<Listino> Listini { get; set; }
        DbSet<Tariffa> Tariffe { get; set; }
        DbSet<Settore> Settori { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public class ListinoDbContext : BaseContext, IListinoDbContext
    {
        public DbSet<Tariffa> Tariffe { get; set; } = null!;
        public DbSet<Settore> Settori { get; set; } = null!;
        public DbSet<Listino> Listini { get; set; } = null!;
    }
}
