using Configurazione.Core.Repository;
using Configurazione.ViewModels.Map;
using System.Diagnostics;

namespace Configurazione.ViewModels
{
    public interface IPostazioneAddViewModel : IConfigurazioneCrudViewModel { }

    public class PostazioneAddViewModel : PostazioneInputBase, IPostazioneAddViewModel
    {
        private IConfigurazionePostazioneRepository Q;

        public PostazioneAddViewModel(IConfigurazionePostazioneRepository Repository) : base()
        {
            Titolo = "Aggiungi Nuova Postazione";
            FieldsVisibile = true;
            FieldsEnabled = true;
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
            BindingT = new ();
        }

        protected override void OnFinalDestruction() => Q = null;

        protected override async Task OnLoading()
        {
            await CaricaCombos();
            await SetFocus(NomeFocus);
            
        }

        private async Task CaricaCombos()
        {
            var dataTipoPostazione = await Q.LoadTipiPostazione();
            TipoPostDataSource = [.. dataTipoPostazione.Select(dto => new ConfigurazioneTipoPostazioneMap(dto))];

            var dataTipoRientro = await Q.LoadTipiRientro();
            TipoRientroDataSource = [.. dataTipoRientro.Select(dto => new ConfigurazioneTipoRientroMap(dto))];


            // Seleziona il primo elemento solo se la lista non è vuota
            if (TipoPostDataSource?.Count > 0)
                BindingT.CodiceTipoPostazione = TipoPostDataSource[0].Id;

            if (TipoRientroDataSource?.Count > 0)
                BindingT.CodiceTipoRientro = TipoRientroDataSource[0].Id;
        }

        protected async override Task OnSaving()
        {
            _isClosing = true;
            // 1. Validazione Dati (ora è un Task, serve await)
            if (!await ValidaDati())
            {

                _isClosing = false; // Permette di riprovare dopo la validazione fallita
                return;
            }

            
            try
            {
                // 2. Controllo Duplicati
                if (await Q.EsisteNome(BindingT.ToDto(),Token))
                {
                    _isClosing = false;
                    InfoLabel = "Postazione già registrata";
                    await SetFocus(NomeFocus);
                    return;
                }

                InfoLabel = "Salvataggio in corso...";

                // 3. Inserimento
                int newPostazioneId = await Q.Add(BindingT.ToDto(), Token);

                if (newPostazioneId == -1)
                {
                    _isClosing = false;
                    InfoLabel = "Errore Db inserimento Postazione";
                    await SetFocus(NomeFocus);
                    return;
                }

                // 4. Successo: Ritorno protetto
                await OnBack(newPostazioneId);
            }
            catch (OperationCanceledException) { Debug.WriteLine("Salvataggio annullato."); _isClosing = false; }
            catch (Exception ex)
            {
                _isClosing = false; 
                InfoLabel = $"Errore: {ex.Message}";
                await SetFocus(NomeFocus);
            }

        }
    }
}
