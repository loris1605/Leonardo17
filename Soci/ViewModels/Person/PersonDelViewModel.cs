using ReactiveUI;
using Soci.Core.Repository;
using Soci.ViewModels.Map;
using System.Diagnostics;

namespace Soci.ViewModels
{
    public interface IPersonDelViewModel : IRoutableViewModel, ISociCrudViewModel { }

    public class PersonDelViewModel : PersonInputBase, IPersonDelViewModel
    {
        private ISociPersonRepository Q;
                
        public PersonDelViewModel(ISociScreen host,
                                  ISociPersonRepository Repository) : base()
        {
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
            _host = host ?? throw new ArgumentNullException(nameof(host));
            
            Titolo = "Elimina Socio";
            FieldsEnabled = false;
            FieldsVisibile = false; ;
                       
        }

        protected override void OnFinalDestruction()
        {
            Q = null;
            DataSource = null;
        }

        protected override async Task OnLoading()
        {
            var data = await Q.FirstPerson(_idDaModificare, Token);

            if (data == null || data.Id == 0)
            {
                InfoLabel = "Errore: Socio non trovato.";
                return; // Esci subito
            }

            BindingT = new SociPersonMap(data);

            Titolo = $"Cancella Socio: {BindingT.Cognome}";

            await SetFocus(EscFocus,0);
        }

        protected async override Task OnSaving()
        {
            _isClosing = true;

            if (BindingT == null || BindingT.Id == 0)
            {
                _isClosing = false;
                InfoLabel = "Errore: Socio non valido.";
                await SetFocus(EscFocus);
                return;
            }

            InfoLabel = "Cancellazione in corso...";

            try
            {
                if (!await Q.Del(BindingT.ToDto(), Token))
                {
                    InfoLabel = "Errore Db eliminazione person";
                    await SetFocus(EscFocus, 0);
                    _isClosing = false;
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
