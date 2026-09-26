using Servizi.Core.Repository;
using System.Diagnostics;

namespace Servizi.ViewModels
{
    public interface IAbbonamentoAddViewModel : IServiziCrudViewModel { }

    public partial class AbbonamentoAddViewModel : AbbonamentoInputBase, IAbbonamentoAddViewModel
    {
        private IServiziAbbonamentoRepository Q;

        public AbbonamentoAddViewModel(IServiziAbbonamentoRepository Repository) : base()
        {
            Titolo = "Aggiungi Nuovo Abbonamento";
            FieldsVisibile = true;
            FieldsEnabled = true;
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
        }

        protected override void OnFinalDestruction()
        {
            Q = null;
            base.OnFinalDestruction();
        }

        protected async override Task OnSaving()
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
                // 2. Controllo Duplicati
                if (await Q.EsisteNome(BindingT.ToDto(), Token))
                {
                    _isClosing = false;
                    InfoLabel = "Tipo Abbonamento già registrato";
                    await SetFocus(NomeFocus);
                    return;
                }

                InfoLabel = "Salvataggio in corso...";

                // 3. Inserimento a Database
                int newTipoAbbonamentoId = await Q.Add(BindingT.ToDto(), Token);

                if (newTipoAbbonamentoId == -1)
                {
                    _isClosing = false;
                    InfoLabel = "Errore Database: inserimento fallito";
                    await SetFocus(NomeFocus);
                    return;
                }

                // 4. Successo: Ritorno protetto
                await OnBack(newTipoAbbonamentoId);
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
