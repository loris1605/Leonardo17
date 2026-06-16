using ReactiveUI;
using Soci.Core.Repository;
using Soci.ViewModels.Map;
using System.Diagnostics;

namespace Soci.ViewModels
{
    public interface ICodiceSocioDelViewModel : IRoutableViewModel, ISociCrudViewModel { }

    public class CodiceSocioDelViewModel : CodiceSocioInputBase, ICodiceSocioDelViewModel
    {
        private ISociPersonRepository Q;
        
        public CodiceSocioDelViewModel(ISociScreen host, ISociPersonRepository Repository) : base()
        {
           FieldsVisibile = false;
           FieldsEnabled = false;

            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        protected override void OnFinalDestruction()
        {
            Q = null;
        }

        protected override async Task OnLoading()
        {
            var data = await Q.FirstSocio(_idDaModificare, Token);
            Token.ThrowIfCancellationRequested();
            if (data == null)
            {
                InfoLabel = "Errore: Socio non trovato nel database.";
                FieldsEnabled = false;

            }
            else
            {
                BindingT = new SociPersonMap(data);
                Titolo = "Elimina Codice Socio : " + GetNumeroSocio;
                Titolo1 = "per " + GetNomeCognome;
            }
            
            await SetFocus(EscFocus);

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
                if (!await Q.DelSocio(BindingT.ToDto(), Token))
                {
                    InfoLabel = "Errore Db eliminazione person";
                    await SetFocus(EscFocus);
                    _isClosing = false;
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Salvataggio annullato.");
                _isClosing = false;
                return;
            }
            catch (Exception ex)
            {
                _isClosing = false;
                InfoLabel = $"Errore: {ex.Message}";
                await SetFocus(EscFocus);
                return;
            }
      
            
            await OnBack(_idRitorno);
        }
    }
}
