using Login.Core.Context;
using Login.Core.DTO;
using Microsoft.EntityFrameworkCore;
using Models.Entity.Global;
using Models.Repository;
using Models.Tables;

namespace Login.Core.Repository
{
    public interface ILoginRepository
    {
        Task<List<LoginDTO>> GetOperatoriAbilitati(CancellationToken ct = default);
        Task SaveSettings(LoginDTO dto, CancellationToken ct = default);
    }

    public class LoginRepository : RepositoryBase<LoginDbContext, Operatore>, ILoginRepository
    {
        public LoginRepository() : base() { }

        public async Task<List<LoginDTO>> GetOperatoriAbilitati(CancellationToken ctk = default)
        {
            return await GetAll(
                selector: LoginDTO.ToLoginDto,
                predicate: p => p.Abilitato == true,
                ct: ctk).ConfigureAwait(false);
        }

        private async Task<List<PostazioneXC>> ListPostazioniByOperatore(int CodiceOperatore, CancellationToken ct)
        {
            return await UsingContextAsync(async ctx =>
                await ctx.Permessi
                         .AsNoTracking()
                         .Where(p => p.OperatoreId == CodiceOperatore)
                         .Select(LoginDTO.ToPostazioneXC)
                         .ToListAsync(ct).ConfigureAwait(false));
        }

        private async Task<List<SettoreXC>> SelectSettoriX(int CodicePostazione, CancellationToken ct)
        {
            return await UsingContextAsync(async ctx =>
                await ctx.Reparti
                         .AsNoTracking()
                         .Where(p => p.PostazioneId == CodicePostazione)
                         .Select(LoginDTO.ToSettoreXC)
                         .ToListAsync(ct).ConfigureAwait(false));
        }

        private async Task<List<TariffaXC>> SelectTariffeX(int CodiceSettore, CancellationToken ct)
        {
            return await UsingContextAsync(async ctx =>
                await ctx.Listini
                         .AsNoTracking()
                         .Where(p => p.SettoreId == CodiceSettore)
                         .Select(LoginDTO.ToTariffaXC)
                         .ToListAsync(ct).ConfigureAwait(false));
        }

        public async Task SaveSettings(LoginDTO dT, CancellationToken ct = default)
        {
            OperatoreXC XOperatore = new()
            {
                IDOPERATORE = dT.Id,
                NOMEOPERATORE = dT.NomeOperatore,
                PASSWORD = dT.Password,
                POSTAZIONI = await ListPostazioniByOperatore(dT.Id, ct).ConfigureAwait(false)
            };

            if (XOperatore.POSTAZIONI?.Count > 0)
            {
                foreach (var postazione in XOperatore.POSTAZIONI)
                {
                    ct.ThrowIfCancellationRequested();
                    postazione.SETTORI = await SelectSettoriX(postazione.CODICEPOSTAZIONE, ct).ConfigureAwait(false);

                    foreach (var settore in postazione.SETTORI)
                    {
                        ct.ThrowIfCancellationRequested();
                        settore.TARIFFE = await SelectTariffeX(settore.CODICESETTORE, ct).ConfigureAwait(false) ?? new List<TariffaXC>();
                    }
                }
            }

            XOperatore.GIORNATA = await GetGiornataOpen(ct).ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();
            GlobalValuesC.MySetting = XOperatore;
        }

        private async Task<GiornataXC> GetGiornataOpen(CancellationToken ct)
        {
            return await UsingContextAsync(async ctx =>
                await ctx.Giornate
                         .AsNoTracking()
                         .Where(x => x.Aperta == true)
                         .Select(LoginDTO.ToGiornataXC)
                         .FirstOrDefaultAsync(ct).ConfigureAwait(false));
        }
    }
}
