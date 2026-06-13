using Microsoft.EntityFrameworkCore;
using Models.Context;
using Models.Tables;

namespace Core.Context
{
    public interface ISettingDbContext
    {
        DbSet<DbSettings> Settings { get; set; }
    }

    public class SettingDbContext : BaseContext, ISettingDbContext
    {
        public DbSet<DbSettings> Settings { get; set; } = null!;
    }
}
