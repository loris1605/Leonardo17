using Configurazione.Core.Repository;
using System.Diagnostics;

namespace Configurazione.ViewModels
{
    public interface IOperatoreDelViewModel : IConfigurazioneCrudViewModel { }

    public class OperatoreDelViewModel : OperatoreInputBase, IOperatoreDelViewModel
    {
        private IConfigurazioneOperatoreRepository Q;
        

        public OperatoreDelViewModel(IConfigurazioneOperatoreRepository Repository) : base()
        {
            
            Titolo = "Cancella Operatore";
            FieldsEnabled = false;
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
        }

        protected override void OnFinalDestruction() => Q = null;

        protected override async Task OnLoading()
        {
            var data = await Q.FirstOperatore(_idDaModificare);

            BindingT = new(data);

            if (BindingT == null || BindingT.Id == 0)
            {
                InfoLabel = "Errore: Operatore non trovato nel database.";
                
                await SetFocus(EscFocus);
                return;
            }

            Titolo = $"Cancella Operatore: {BindingT.NomeOperatore}";
            
            await SetFocus(EscFocus);
        }

        protected async override Task OnSaving()
        {
            _isClosing = true;

            if (BindingT == null || BindingT.Id == 0)
            {
                _isClosing = false;
                InfoLabel = "Errore: Operatore non valido.";
                await SetFocus(EscFocus);
                return;
            }

            InfoLabel = "Cancellazione in corso...";

            try
            {
                if (!await Q.Del(BindingT.ToDto(), Token))
                {
                    _isClosing = false;
                    InfoLabel = "Errore Db eliminazione operatore";
                    await SetFocus(EscFocus);
                    return;
                }
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
                InfoLabel = $"Errore: {ex.Message}";
                await SetFocus(EscFocus);
            }
           
            
        }
    }
}
