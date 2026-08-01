using Configurazione.Core.Repository;
using Configurazione.ViewModels.Map;
using System.Diagnostics;

namespace Configurazione.ViewModels
{
    public interface ISettoreAddViewModel : IConfigurazioneCrudViewModel { }

    public class SettoreAddViewModel : SettoreInputBase, ISettoreAddViewModel
    {

        private IConfigurazioneSettoreRepository Q;

        public SettoreAddViewModel(IConfigurazioneSettoreRepository Repository) : base()
        {
            Titolo = "Aggiungi Nuovo Settore";
            FieldsVisibile = true;
            FieldsEnabled = true;
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
            BindingT = new ();
        }

        protected override void OnFinalDestruction() => Q = null;

        protected override async Task OnLoading()
        {
            await CaricaCombos();
            await SetFocus(NomeFocus);
        }

        private async Task CaricaCombos()
        {
            var data = await Q.LoadTipiSettore();
            TipoSettDataSource = [.. data.Select(dto => new ConfigurazioneTipoSettoreMap(dto))];
            // Seleziona il primo elemento come default in modo sicuro
            if (TipoSettDataSource?.Count > 0)
            {
                BindingT.CodiceTipoSettore = TipoSettDataSource[0].Id;
            }
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
                    InfoLabel = "Settore già registrato";
                    await SetFocus(NomeFocus);
                    return;
                }

                InfoLabel = "Salvataggio in corso...";

                // 3. Inserimento a Database
                int newSettoreId = await Q.Add(BindingT.ToDto(), Token);

                if (newSettoreId == -1)
                {
                    _isClosing = false;
                    InfoLabel = "Errore Database: inserimento fallito";
                    await SetFocus(NomeFocus);
                    return;
                }

                // 4. Successo: Ritorno protetto
                await OnBack(newSettoreId);
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
