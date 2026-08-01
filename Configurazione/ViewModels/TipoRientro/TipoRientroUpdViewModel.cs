using Configurazione.Core.Repository;
using System.Diagnostics;

namespace Configurazione.ViewModels
{
    public interface ITipoRientroUpdViewModel : IConfigurazioneCrudViewModel { }

    public class TipoRientroUpdViewModel : TipoRientroInputBase, ITipoRientroUpdViewModel
    {
        private ITipoRientroRepository Q;

        public TipoRientroUpdViewModel(ITipoRientroRepository Repository) : base()
        {
            Titolo = "Modifica Tipo Rientro";
            FieldsVisibile = true;
            FieldsEnabled = true;
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
        }

        protected override void OnFinalDestruction() => Q = null;

        protected override async Task OnLoading()
        {
            try
            {
                var data = await Q.FirstTipoRientro(_idDaModificare);
                if (data == null)
                {
                    InfoLabel = "Errore: Tipo Rientro non trovato.";
                    FieldsEnabled = false;
                    return;
                }
                BindingT = new (data);
                // In modifica, portiamo il focus sul nome all'avvio
                await SetFocus(NomeFocus);
            }
            catch (Exception ex)
            {
                InfoLabel = $"Errore caricamento: {ex.Message}";
                FieldsEnabled = false;
            }
        }

        protected async override Task OnSaving()
        {
            InfoLabel = "";
            _isClosing = true;
            // 1. Validazione (attesa perché async nella base)
            if (!await ValidaDati())
            {
                _isClosing = false;
                return;
            }
            try
            {
                // 2. Controllo Duplicati
                if (await Q.EsisteNome(BindingT.ToDto(), Token))
                {
                    _isClosing = false;
                    InfoLabel = "Tipo Rientro già registrato";
                    await SetFocus(NomeFocus);
                    return;
                }
                InfoLabel = "Salvataggio in corso...";
                // 3. Aggiornamento a Database
                if (!await Q.Upd(BindingT.ToDto(), Token))
                {
                    _isClosing = false;
                    InfoLabel = "Errore Database: aggiornamento fallito";
                    await SetFocus(NomeFocus);
                    return;
                }
                await OnBack(_idDaModificare);
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
