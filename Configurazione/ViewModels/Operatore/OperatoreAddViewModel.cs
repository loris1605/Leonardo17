using Configurazione.Core.Repository;
using ReactiveUI;
using System.Diagnostics;

namespace Configurazione.ViewModels
{
    public interface IOperatoreAddViewModel : IRoutableViewModel, IConfigurazioneCrudViewModel { }

    public class OperatoreAddViewModel : OperatoreInputBase, IOperatoreAddViewModel
    {
        private IConfigurazioneOperatoreRepository Q;
        
        public OperatoreAddViewModel( IConfigurazioneScreen host,
                                      IConfigurazioneOperatoreRepository Repository) : base()
        {
            
            Titolo = "Aggiungi Nuovo Operatore";
            FieldsVisibile = true;
            FieldsEnabled = true;
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
            _host = host ?? throw new ArgumentNullException(nameof(host));
            BindingT = new();
        }

        protected override void OnFinalDestruction()
        {
            Q = null;
            _host = null;
        }

        protected override async Task OnLoading() => await SetFocus(NomeFocus);

        protected async override Task OnSaving()
        {
            _isClosing = true;
            // 1. Validazione Dati (ora è un Task, serve await)
            if (!await ValidaDati())
            { 
          
                _isClosing = false; // Permette di riprovare dopo la validazione fallita
                return;
            }

            // 2. Controllo duplicati con CancellationToken (ereditato dalla base)
            try
            {
                if (await Q.EsisteNome(BindingT.ToDto(), Token))
                {
                    _isClosing = false;
                    InfoLabel = "Operatore già registrato";
                    await SetFocus(NomeFocus);
                    return;
                }

                InfoLabel = "Salvataggio in corso...";

                // 3. Impostazioni pre-salvataggio
                BindingT.CodicePerson = -2; // Logica specifica per l'anagrafica operatore

                // 4. Inserimento a Database
                int newOperatoreId = await Q.Add(BindingT.ToDto(), Token);

                if (newOperatoreId == -1)
                {
                    _isClosing = false;
                    InfoLabel = "Errore Db inserimento Operatore";
                    await SetFocus(NomeFocus);
                    return;
                }

                // 5. Ritorno alla grid (ora è un Task asincrono e protetto)
                await OnBack(newOperatoreId);
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
                await SetFocus(NomeFocus);
            }
        }
    }
}
