using Cassa.Core.Context;
using Cassa.Core.DTO;
using Microsoft.EntityFrameworkCore;
using Models.Repository;
using Models.Tables;
using System.Diagnostics;

namespace Cassa.Core.Repository
{
    public interface IEntraSocioRepository
    {
        Task<EntraSocioDTO> GetPersonByTessera(string numeroTessera, CancellationToken ctk = default);
        Task<List<EntraIngressiDTO>> GetIngressiByPostazione(int postazioneId, CancellationToken ctk = default);
        Task<int> AddNewScheda(EntraSocioDTO dto, CancellationToken ctk = default);
        Task<bool> EsisteSocioInside(EntraSocioDTO dto, CancellationToken ctk = default);
    }

    public class EntraSocioRepository(IEntraSocioDbContext ctx) : BaseRepository<EntraSocioDbContext, Scheda>, IEntraSocioRepository
    {
        private readonly IEntraSocioDbContext _ctx = ctx;

        public async Task<EntraSocioDTO> GetPersonByTessera(string numeroTessera, CancellationToken ctk = default)
        {
            var data = await _ctx.Tessere
                .Where(t => t.NumeroTessera == numeroTessera)
                .Include(t => t.Socio)
                    .ThenInclude(s => s.Person)
                .FirstOrDefaultAsync(ctk);

            if (data == null || data.Socio == null || data.Socio.Person == null) return new EntraSocioDTO();

            return new EntraSocioDTO
            {
                NumeroTessera = data.NumeroTessera,
                CodicePerson = data.Socio.Person.Id,
                Cognome = data.Socio.Person.SurName,
                Nome = data.Socio.Person.FirstName,
                Natoil = data.Socio.Person.Natoil,
                CodiceUnivoco = data.Socio.Person.UniqueParam,
                Blocco = !data.Abilitato,
                NumeroSocio = data.Socio.NumeroSocio,
                ScadenzaTessera = data.Scadenza
            };
        }

        public async Task<List<EntraIngressiDTO>> GetIngressiByPostazione(int postazioneId, CancellationToken ctk = default)
        {
            var data = await _ctx.Tariffe
                .AsNoTracking() // Ottimizza le performance se devi solo mostrare i dati
                .Where(t => t.Listini.Any(l =>
                    l.Settore != null &&
                    l.Settore.TipoSettoreId == -1 &&
                    l.Settore.Reparti.Any(r => r.PostazioneId == postazioneId)
                ))
                .ToListAsync(ctk);

            return [.. data.Select(t => new EntraIngressiDTO
            {
                Id = t.Id, //id tariffa
                NomeTariffa = t.Nome,
                EtichettaTariffa = t.Label,
                PrezzoTariffa = t.Prezzo,
                IsFreeDrink = t.IsFreeDrink
            })];

        }

        public async Task<int> AddNewScheda(EntraSocioDTO dto, CancellationToken ctk = default)
        {
            var scheda = new Scheda
            {
                Posizione = dto.Posizione, // Imposta la posizione desiderata
                NumeroTessera = dto.NumeroTessera,
                PersonId = dto.CodicePerson,
                Cognome = dto.Cognome,
                Nome = dto.Nome,
                Natoil = dto.Natoil,
                CheckinTime = DateTime.Now,
                CheckoutTime = DateTime.MaxValue, // Imposta un valore predefinito per il checkout
                Consumazione = dto.Consumazione,
                Blocco = dto.Blocco,
                Note = dto.Note
            };



            await _ctx.Schede.AddAsync(scheda, ctk);

            try
            {
                await _ctx.SaveChangesAsync(ctk);
                return scheda.Id;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Errore Add: {ex.InnerException?.Message ?? ex.Message}");
                return -1;
            }
        }

        public async Task<bool> EsisteSocioInside(EntraSocioDTO dto, CancellationToken ctk = default)
        {
            return await _ctx.Schede.Where(x => x.PersonId == dto.CodicePerson).AnyAsync(ctk);
        }
    }
}
