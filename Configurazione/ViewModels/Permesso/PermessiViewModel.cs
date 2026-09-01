using Configurazione.Core.Repository;
using Configurazione.ViewModels.Map;
using ReactiveUI;
using System.Diagnostics;

namespace Configurazione.ViewModels
{
    public interface IPermessoViewModel : IConfigurazioneCrudViewModel { }

    public partial class PermessiViewModel(IPermessoRepository Repository, 
                                           IConfigurazioneOperatoreRepository oRepository) : OperatoreInputBase(), IPermessoViewModel
    {
        private IPermessoRepository Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
        private IConfigurazioneOperatoreRepository oQ = oRepository ?? throw new ArgumentNullException(nameof(oRepository));

        protected override void OnFinalDestruction()
        {
            Q = null;
            oQ = null;
            base.OnFinalDestruction();
        }

        protected override async Task OnLoading()
        {
            // 1. Recupero dati operatore per il titolo
            var operatore = await oQ.FirstOperatore(_idDaModificare, Token);
            Titolo = $"Permessi per l'operatore: {operatore?.NomeOperatore ?? "Sconosciuto"}";

            // 2. Caricamento della lista dei permessi
            var data = await Q.GetPermessi(_idDaModificare, Token);

            // Trasformiamo i DTO in Map per il binding con le CheckBox nella Grid
            DataSource = [.. data.Select(dto => new ConfigurazionePostazioneElencoMap(dto))];

            // 3. Focus asincrono
            await SetFocus(EscFocus);
        }

        protected async override Task OnSaving()
        {
            if (DataSource == null) return;

            _isClosing = true;

            // Trasformiamo tutta la lista modificata di nuovo in DTO
            var dtoSource = DataSource.Select(p => p.ToDto()).ToList();

            try
            {
                InfoLabel = "Salvataggio permessi...";

                if (!await Q.SavePermessi(_idDaModificare, dtoSource, Token))
                {
                    InfoLabel = "Errore Database: modifica permessi fallita";
                    _isClosing = false;
                    await SetFocus(EscFocus);
                    return;
                }

                // Successo: ritorno protetto
                await OnBack(_idDaModificare);
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

    public partial class PermessiViewModel
    {
        #region DataSource

        private IList<ConfigurazionePostazioneElencoMap> _datasource = [];
        public IList<ConfigurazionePostazioneElencoMap> DataSource
        {
            get => _datasource;
            set => this.RaiseAndSetIfChanged(ref _datasource, value);
        }

        #endregion

        #region BindingT

        private ConfigurazionePostazioneElencoMap _bindingT;
        public new ConfigurazionePostazioneElencoMap BindingT
        {
            get => _bindingT;
            set => this.RaiseAndSetIfChanged(ref _bindingT, value);
        }

        #endregion


    }
}
