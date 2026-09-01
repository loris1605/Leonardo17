using Configurazione.Core.Repository;
using System.Diagnostics;

namespace Configurazione.ViewModels
{
    public interface ITariffaDelViewModel : IConfigurazioneCrudViewModel { }

    public class TariffaDelViewModel : TariffaInputBase, ITariffaDelViewModel
    {
        private IConfigurazioneTariffaRepository Q;

        public TariffaDelViewModel(IConfigurazioneTariffaRepository Repository) : base()
        {
            Titolo = "Cancella Settore";
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
            FieldsEnabled = false;
        }

        protected override void OnFinalDestruction() => Q = null;

        protected override async Task OnLoading()
        {
            var data = await Q.FirstTariffa(_idDaModificare);

            BindingT = new (data);

            if (GetCodiceTariffa == 0)
            {
                InfoLabel = "Errore: Tariffa non trovata nel database.";
                FieldsEnabled = false;
            }
            await SetFocus(EscFocus);
        }

        protected async override Task OnSaving()
        {
            _isClosing = true;

            if (BindingT == null || BindingT.Id == 0)
            {
                _isClosing = false;
                InfoLabel = "Errore: Tariffa non valida.";
                await SetFocus(EscFocus);
                return;
            }

            InfoLabel = "Cancellazione in corso...";

            try
            {
                // Esecuzione eliminazione
                if (!await Q.Del(BindingT.ToDto(), Token))
                {
                    _isClosing = false;
                    InfoLabel = "Errore Database: impossibile eliminare la tariffa";
                    await SetFocus(EscFocus);
                    return;
                }

                // Successo: ritorno alla grid con flag di refresh totale
                await OnBack(-100);
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Salvataggio annullato.");
                _isClosing = false;
            }
            catch (Exception ex)
            {
                _isClosing = false;
                InfoLabel = $"Errore critico: {ex.Message}";
                await SetFocus(EscFocus);
            }
        }
    }
}
