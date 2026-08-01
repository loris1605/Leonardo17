using Configurazione.Core.Repository;
using Configurazione.ViewModels.Map;
using System.Diagnostics;

namespace Configurazione.ViewModels
{
    public interface IPostazioneDelViewModel : IConfigurazioneCrudViewModel { }

    public class PostazioneDelViewModel : PostazioneInputBase, IPostazioneDelViewModel
    {
        private IConfigurazionePostazioneRepository Q;
       
        public PostazioneDelViewModel(IConfigurazionePostazioneRepository Repository) : base()
        {
            
            Titolo = "Cancella Postazione";

            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));

            // In cancellazione disabilitiamo l'input ma mostriamo i dati
            FieldsEnabled = false;
            FieldsVisibile = true;
        }

        protected override void OnFinalDestruction() => Q = null;

        protected override async Task OnLoading()
        {
            var dataTipoPostazione = await Q.LoadTipiPostazione();
            TipoPostDataSource = [.. dataTipoPostazione.Select(dto => new ConfigurazioneTipoPostazioneMap(dto))];

            var dataTipoRientro = await Q.LoadTipiRientro();
            TipoRientroDataSource = [.. dataTipoRientro.Select(dto => new ConfigurazioneTipoRientroMap(dto))];

            var data = await Q.FirstPostazione(_idDaModificare);
            BindingT = new (data);
            Titolo = $"Cancella Postazione: {BindingT.NomePostazione}";


            if (GetCodicePostazione == 0)
            {
                InfoLabel = "Errore: Postazione non trovata nel database.";
                FieldsEnabled = false;
                await SetFocus(EscFocus);
                return;
            }
            await SetFocus(EscFocus);
        }

        protected async override Task OnSaving()
        {
            _isClosing = true;

            if (BindingT == null || BindingT.Id == 0)
            {
                _isClosing = false;
                InfoLabel = "Errore: Postazione non valida.";
                await SetFocus(EscFocus);
                return;
            }

            try
            {
                // Esecuzione eliminazione
                if (!await Q.Del(BindingT.ToDto(), Token))
                {
                    _isClosing = false;
                    InfoLabel = "Errore Database: impossibile eliminare la postazione";
                    await SetFocus(EscFocus);
                    return;
                }

                InfoLabel = "Cancellazione in corso...";

                // Successo: refresh totale della grid (-100)
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
