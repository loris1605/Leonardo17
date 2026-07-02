using Microsoft.EntityFrameworkCore;
using Models.Context;
using Models.Tables;

namespace Configurazione.Core.Context
{
    public interface ITipoRientroDbContext
    {
        DbSet<TipoRientro> TipiRientro { get; set; }
    }

    public class TipoRientroDbContext : BaseContext, ITipoRientroDbContext
    {
        public DbSet<TipoRientro> TipiRientro { get; set; } = null!;
    }
}
