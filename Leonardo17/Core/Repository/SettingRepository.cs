using Core.Context;
using Microsoft.EntityFrameworkCore;
using Models.Context;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Core.Repository
{
    public interface ISettingRepository
    {
        Task<bool> CheckAppVersion(int appVersion);
    }

    public class SettingRepository : ISettingRepository
    {
        private readonly ISettingDbContext _dbx;

        public SettingRepository(ISettingDbContext dbx)
        {
            _dbx = dbx;
            Debug.WriteLine($"***** [GC] {this.GetType().Name} {this.GetHashCode()} CARICATO *****");
        }

#if DEBUG
        ~SettingRepository()
        {
            // Questo apparirà nella finestra "Output" di Visual Studio
            Debug.WriteLine($"***** [GC] {this.GetType().Name} {this.GetHashCode()} DISTRUTTO *****");
        }
#endif

        public async Task<bool> CheckAppVersion(int appVersion)
        {
            return await _dbx.Settings
                        .AsNoTracking()
                        .AnyAsync(s => s.Version == appVersion);
        }

        public async Task CheckDataBaseExist()
        {
            using AppDbContext _dbx = new AppDbContext();
            bool dbExists = await _dbx.Database.CanConnectAsync();
            if (!dbExists)
            {
                Debug.WriteLine("Database non trovato. Creazione del database...");
                await _dbx.Database.EnsureCreatedAsync();
                Debug.WriteLine("Database creato con successo.");
            }
            else
            {
                Debug.WriteLine("Database esistente. Nessuna azione necessaria.");
            }
        }
    }
}
