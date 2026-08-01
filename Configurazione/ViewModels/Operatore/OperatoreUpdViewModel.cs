using Configurazione.Core.Repository;
using System.Diagnostics;

namespace Configurazione.ViewModels
{
    public interface IOperatoreUpdViewModel : IConfigurazioneCrudViewModel { }

    public class OperatoreUpdViewModel : OperatoreInputBase, IOperatoreUpdViewModel
    {
        private IConfigurazioneOperatoreRepository Q;
        
        public OperatoreUpdViewModel(IConfigurazioneOperatoreRepository Repository) : base()
        {
            
            Titolo = "Modifica Operatore";
            
            FieldsEnabled = true;
            FieldsVisibile = true;

            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
        }

        
        protected override void OnFinalDestruction() => Q = null;
        

        protected override async Task OnLoading()
        {
            var data = await Q.FirstOperatore(_idDaModificare);

            BindingT = new(data);

            if (BindingT == null || BindingT.Id == 0)
            {
                InfoLabel = "Errore: Operatore non trovato.";
                FieldsEnabled = false;
                await SetFocus(EscFocus);
                return;
            }


            NomeOperatoreEnabled = _idDaModificare != -1;
            if (NomeOperatoreEnabled)
            {
                await SetFocus(NomeFocus);
            }
            else
            {
                await SetFocus(PasswordFocus);
            }
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
                if (await Q.EsisteNomeUpd(BindingT.ToDto(), Token))
                {
                    _isClosing = false;
                    InfoLabel = "Nome operatore già in uso da un altro utente";
                    await SetFocus(NomeFocus);
                    return;
                }

                InfoLabel = "Aggiornamento in corso...";

                // 3. Esecuzione Update
                if (!await Q.Upd(BindingT.ToDto(), Token))
                {
                    _isClosing = false;
                    InfoLabel = "Errore Db durante la modifica";
                    await SetFocus(NomeFocus);
                    return;
                }

                // 4. Successo: Ritorno alla grid
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
