using Configurazione.Core.Repository;
using Configurazione.ViewModels.Map;
using System.Diagnostics;

namespace Configurazione.ViewModels
{
    public interface IPostazioneUpdViewModel : IConfigurazioneCrudViewModel { }

    public class PostazioneUpdViewModel : PostazioneInputBase, IPostazioneUpdViewModel
    {
        private IConfigurazionePostazioneRepository Q;

        public PostazioneUpdViewModel(IConfigurazionePostazioneRepository Repository) : base()
        {
            Titolo = "Modifica Postazione";
            FieldsEnabled = true;
            FieldsVisibile = true;
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
        }

        protected override void OnFinalDestruction() => Q = null;

        protected override async Task OnLoading()
        {

            var dataTipoPostazione = await Q.LoadTipiPostazione();
            TipoPostDataSource = [.. dataTipoPostazione.Select(dto => new ConfigurazioneTipoPostazioneMap(dto))];

            var dataTipoRientro = await Q.LoadTipiRientro();
            TipoRientroDataSource = [.. dataTipoRientro.Select(dto => new ConfigurazioneTipoRientroMap(dto))];

            var data = await Q.FirstPostazione(_idDaModificare);
            BindingT = new (data);

            if (BindingT == null || BindingT.Id == 0)
            {
                InfoLabel = "Errore: Postazione non trovata.";
                FieldsEnabled = false;
                await SetFocus(EscFocus);
                return;
            }

            // 3. Focus iniziale sul nome
            await SetFocus(NomeFocus);
        }

        protected override async Task OnSaving()
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
                // 2. Controllo Duplicati (escludendo se stesso tramite Dto/Id)
                if (await Q.EsisteNomeUpd(BindingT.ToDto(), Token))
                {
                    _isClosing = false;
                    InfoLabel = "Nome postazione già in uso da un'altra postazione";
                    await SetFocus(NomeFocus);
                    return;
                }

                InfoLabel = "Aggiornamento in corso...";

                // 3. Esecuzione Update
                if (!await Q.Upd(BindingT.ToDto(), Token))
                {
                    _isClosing = false;
                    InfoLabel = "Errore Database durante la modifica";
                    await SetFocus(NomeFocus);
                    return;
                }

                // 4. Successo: Ritorno alla grid con l'ID modificato
                await OnBack(_idDaModificare);
            }
            catch (OperationCanceledException)
            {
                _isClosing = false;
                Debug.WriteLine("Salvataggio annullato.");
            }
            catch (Exception ex)
            {
                _isClosing = false;
                InfoLabel = $"Errore: {ex.Message}";
                await SetFocus(NomeFocus);
            }

        }
    }
}
