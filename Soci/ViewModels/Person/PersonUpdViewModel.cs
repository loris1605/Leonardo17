using ReactiveUI;
using Soci.Core.Repository;
using Soci.ViewModels.Map;

namespace Soci.ViewModels
{
    public interface IPersonUpdViewModel : IRoutableViewModel, ISociCrudViewModel { }

    public class PersonUpdViewModel : PersonInputBase, IPersonUpdViewModel
    {
        private ISociPersonRepository Q;
               

        public PersonUpdViewModel(ISociScreen host, ISociPersonRepository Repository) : base()
        {
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
            _host = host ?? throw new ArgumentNullException(nameof(host));
            Titolo = "Modifica Socio";
            FieldsEnabled = true;
            FieldsVisibile = false;
                        
        }

        protected override void OnFinalDestruction()
        {
            Q = null;
            DataSource = null;
        }

        protected override async Task OnLoading()
        {
            var data = await Q.FirstPerson(_idDaModificare, Token);
            BindingT = new SociPersonMap(data);

            if (BindingT == null || BindingT.Id == 0)
            {
                InfoLabel = "Errore: Socio non trovato.";
                FieldsEnabled = false;
                return;
            }
            await SetFocus(CognomeFocus);

        }

        protected override async Task OnSaving()
        {
            _isClosing = true;

            if (!await ValidaDati())
            {
                _isClosing = false; // Permette di riprovare dopo la validazione fallita
                await SetFocus(NomeFocus);
                await SetFocus(CognomeFocus); // resta NomeFocus se CognomeBox è disabled altrimenti passa a CognomeFocus
                return;
            }

            try
            {
                if (await EsisteAnagraficaUpd())
                {
                    _isClosing = false;
                    InfoLabel = "Socio già registrato";
                    await SetFocus(NomeFocus);
                    await SetFocus(CognomeFocus);
                    return;
                }

                InfoLabel = "Updating Database...";

                if (!await Q.UpdPerson(BindingT.ToDto(), Token))
                {
                    _isClosing = false;
                    InfoLabel = "Errore Db modifica person";
                    await SetFocus(NomeFocus);
                    await SetFocus(CognomeFocus);
                    return;
                }

                await OnBack(_idDaModificare);
            }
            catch (OperationCanceledException) { _isClosing = false; }
            catch (Exception ex)
            {
                _isClosing = false;
                InfoLabel = $"Errore: {ex.Message}";
                await SetFocus(NomeFocus);
            }
            
        }

        private async Task<bool> EsisteAnagraficaUpd()
        {
            string srvcognome = (GetCognome ?? "").PadRight(3);
            string srvnome = (GetNome ?? "").PadRight(3);

            BindingT.CodiceUnivoco = string.Concat(
                                    srvcognome[..3],
                                    srvnome[..3],
                                    BindingT.Natoil.ToString());

            try
            {
                return await Q.EsisteCodiceUnivoco(CodiceUnivoco, BindingT.Id, Token);
            }
            catch (OperationCanceledException)
            {
                InfoLabel = "Operazione annullata dall'utente";
                
            }
            catch (Exception ex)
            {
                InfoLabel = $"Esiste Anagrafica Error: {ex.Message}";
            }

            return false;
         

        }
    }
}
