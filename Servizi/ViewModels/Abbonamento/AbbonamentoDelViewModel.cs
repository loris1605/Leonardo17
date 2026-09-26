using Servizi.Core.Repository;
using System.Diagnostics;

namespace Servizi.ViewModels
{
    public interface IAbbonamentoDelViewModel : IServiziCrudViewModel { }

    public partial class AbbonamentoDelViewModel : AbbonamentoInputBase, IAbbonamentoDelViewModel
    {
        private IServiziAbbonamentoRepository Q;
        public AbbonamentoDelViewModel(IServiziAbbonamentoRepository Repository) : base()
        {
            Titolo = "Cancella Tipo Abbonamento";
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
            FieldsEnabled = false;
        }
        protected override void OnFinalDestruction()
        {
            Q = null;
            base.OnFinalDestruction();
        }

        protected override async Task OnLoading()
        {
            var data = await Q.FirstAbbonamento(_idDaModificare);

            BindingT = new(data);

            if (GetCodiceAbbonamento == 0)
            {
                InfoLabel = "Errore: Tipo Abbonamento non trovato nel database.";
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
                InfoLabel = "Errore: Tipo Abbonamento non valido.";
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
                    InfoLabel = "Errore Database: impossibile eliminare il Tipo Abbonamento";
                    await SetFocus(EscFocus);
                    return;
                }

                // Successo: ritorno alla grid con flag di refresh totale
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
                InfoLabel = $"Errore critico: {ex.Message}";
                await SetFocus(EscFocus);
            }
        }

    }
}
