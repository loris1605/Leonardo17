using Configurazione.Core.Repository;
using System.Diagnostics;

namespace Configurazione.ViewModels
{
    public interface ITipoRientroDelViewModel : IConfigurazioneCrudViewModel { }

    public class TipoRientroDelViewModel : TipoRientroInputBase, ITipoRientroDelViewModel
    {
        private ITipoRientroRepository Q;

        public TipoRientroDelViewModel(ITipoRientroRepository Repository) : base()
        {
            Titolo = "Cancella Tipo Rientro";
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
            FieldsEnabled = false;
        }

        protected override void OnFinalDestruction() => Q = null;

        protected override async Task OnLoading()
        {
            var data = await Q.FirstTipoRientro(_idDaModificare);
            BindingT = new (data);
            if (GetCodiceTipoRientro == 0)
            {
                InfoLabel = "Errore: Tipo Rientro non trovato nel database.";
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
                InfoLabel = "Errore: Tipo Rientro non valido.";
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
                    InfoLabel = "Errore Database: impossibile eliminare il Tipo Rientro";
                    await SetFocus(EscFocus);
                    return;
                }
                // Successo: ritorno alla grid con flag di refresh totale
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Salvataggio annullato.");
                _isClosing = false;
            }
            catch (Exception ex)
            {
                _isClosing = false;
                InfoLabel = $"Errore: {ex.Message}";
                await SetFocus(EscFocus);
            }
        }




    }
}
