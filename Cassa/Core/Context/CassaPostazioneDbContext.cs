using Microsoft.EntityFrameworkCore;
using Models.Context;
using Models.Tables;

namespace Cassa.Core.Context
{
    public interface ICassaPostazioneDbContext
    {
        DbSet<Postazione> Postazioni { get; set; }
        DbSet<Scheda> Schede { get; set; }
    }

    public class CassaPostazioneDbContext : BaseContext, ICassaPostazioneDbContext
    {
        public DbSet<Postazione> Postazioni { get; set; } = null;
        public DbSet<Scheda> Schede { get; set; } = null!;
    }
}
