using Servizi.Core.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servizi.ViewModels
{
    public interface IAbbonamentoUpdViewModel : IServiziCrudViewModel { }

    public partial class AbbonamentoUpdViewModel : AbbonamentoInputBase, IAbbonamentoDelViewModel
    {
        private IServiziAbbonamentoRepository Q;

        public AbbonamentoUpdViewModel(IServiziAbbonamentoRepository Repository) : base()
        {
            Titolo = "Modifica Tipo Abbonamento";
            FieldsVisibile = true; // Impostato come richiesto
            FieldsEnabled = true;

            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
        }

        protected override void OnFinalDestruction()
        {
            Q = null;
            base.OnFinalDestruction();
        }

        protected override async Task OnLoading()
        {
            try
            {
                var data = await Q.FirstAbbonamento(_idDaModificare);

                if (data == null)
                {
                    InfoLabel = "Errore: Tipo Abbonamento non trovato.";
                    FieldsEnabled = false;
                    return;
                }

                BindingT = new(data);

                // In modifica, portiamo il focus sul nome all'avvio
                await SetFocus(NomeFocus);
            }
            catch (Exception ex)
            {
                InfoLabel = $"Errore caricamento: {ex.Message}";
                FieldsEnabled = false;
            }
        }

        protected override async Task OnSaving()
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
                // 2. Disabilita UI durante l'operazione
                FieldsEnabled = false;

                var input = BindingT.ToDto();

                // 3. Controllo duplicati (escludendo se stessa)
                if (await Q.EsisteNomeUpd(input))
                {
                    _isClosing = false;
                    InfoLabel = "Tipo Abbonamento già registrato";
                    FieldsEnabled = true;
                    await SetFocus(NomeFocus);
                    return;
                }

                InfoLabel = "Aggiornamento in corso...";

                // 4. Aggiornamento Database
                if (!await Q.Upd(input))
                {
                    _isClosing = false;
                    InfoLabel = "Errore Db modifica Tipo Abbonamento";
                    FieldsEnabled = true;
                    await SetFocus(NomeFocus);
                    return;
                }

                // 5. Successo
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
                FieldsEnabled = true;
                await SetFocus(NomeFocus);
            }
        }
    }
}
