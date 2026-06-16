using ReactiveUI;
using Soci.Core.Repository;
using Soci.ViewModels.Map;
using System.Diagnostics;

namespace Soci.ViewModels
{
    public interface ITesseraDelViewModel : IRoutableViewModel, ISociCrudViewModel { }

    public class TesseraDelViewModel : TesseraInputBase, ITesseraDelViewModel
    {
        private ISociPersonRepository Q;
        public TesseraDelViewModel(ISociScreen host, ISociPersonRepository Repository) : base()
        {
           FieldVisibile = false;
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
            var data = await Q.FirstTessera(_idDaModificare, Token);

            if (data == null)
            {
                InfoLabel = "Errore: Tesera non trovata nel database.";
                FieldsEnabled = false;
            }
            else
            {
                BindingT = new SociPersonMap(data);
                Titolo = "Elimina Tessera : " + GetNumeroTessera;
                Titolo1 = "per " + GetNomeCognome;
            }

            await SetFocus(EscFocus);

        }

        protected async override Task OnSaving()
        {
            _isClosing = true;

            try
            {
                InfoLabel = "Salvataggio in corso...";

                if (!await Q.DelTessera(BindingT.ToDto(), Token))
                {
                    _isClosing = false;
                    InfoLabel = "Errore Db eliminazione person";
                    await SetFocus(EscFocus);
                    return;
                }

                await OnBack(_idRitorno);
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
