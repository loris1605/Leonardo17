using Configurazione.Core.Repository;
using Configurazione.ViewModels.Map;
using System.Diagnostics;

namespace Configurazione.ViewModels
{
    public interface ITariffaAddViewModel : IConfigurazioneCrudViewModel { }

    public class TariffaAddViewModel : TariffaInputBase, ITariffaAddViewModel
    {
        private IConfigurazioneTariffaRepository Q;

        public TariffaAddViewModel(IConfigurazioneTariffaRepository Repository) : base()
        {
            Titolo = "Aggiungi Nuova Tariffa";
            FieldsVisibile = true;
            FieldsEnabled = true;
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
        }

        protected override void OnFinalDestruction() => Q = null;

        protected override async Task OnLoading()
        {
            await SetFocus(NomeFocus);
            
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
                if (await Q.EsisteNome(BindingT.ToDto(), Token))
                {
                    _isClosing = false;
                    InfoLabel = "Tariffa già registrata";
                    await SetFocus(NomeFocus);
                    return;
                }

                InfoLabel = "Salvataggio in corso...";

                // 3. Inserimento a Database
                int newTariffaId = await Q.Add(BindingT.ToDto(), Token);

                if (newTariffaId == -1)
                {
                    _isClosing = false;
                    InfoLabel = "Errore Database: inserimento fallito";
                    await SetFocus(NomeFocus);
                    return;
                }

                // 4. Successo: Ritorno protetto
                await OnBack(newTariffaId);
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
