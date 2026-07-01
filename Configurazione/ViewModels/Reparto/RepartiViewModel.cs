using Configurazione.Core.Repository;
using Configurazione.ViewModels.Map;
using ReactiveUI;
using System.Diagnostics;

namespace Configurazione.ViewModels
{
    public interface IRepartoViewModel : IConfigurazioneCrudViewModel { }

    public partial class RepartiViewModel(IConfigurazionePostazioneRepository pRepository, 
                                          IConfigurazioneRepartoRepository Repository) : 
                                          PostazioneInputBase(), IRepartoViewModel
    {
        private IConfigurazioneRepartoRepository Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
        private IConfigurazionePostazioneRepository pQ = pRepository ?? throw new ArgumentNullException(nameof(pRepository));

        protected override void OnFinalDestruction()
        {
            Q = null;
            pQ = null;
            base.OnFinalDestruction();
        }

        protected override async Task OnLoading()
        {
            var postazione = await pQ.FirstPostazione(_idDaModificare, Token);
            Titolo = $"Reparti per la postazione: {postazione?.NomePostazione ?? "Sconosciuto"}";
            var data = await Q.GetReparti(_idDaModificare, Token);
            
            DataSource = [.. data.Select(dto => new ConfigurazioneSettoreElencoMap(dto))];
            await SetFocus(EscFocus);
        }

        protected async override Task OnSaving()
        {
            if (DataSource == null) return;
            _isClosing = true;

            var dtoSource = DataSource.Select(p => p.ToDto()).ToList();

            try
            {
                InfoLabel = "Salvataggio reparti...";

                if (!await Q.SaveReparti(_idDaModificare, dtoSource, Token))
                {
                    InfoLabel = "Errore Database: modifica reparti fallita";
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

    public partial class RepartiViewModel
    {
        #region DataSource

        private IList<ConfigurazioneSettoreElencoMap> _datasource = [];
        public IList<ConfigurazioneSettoreElencoMap> DataSource
        {
            get => _datasource;
            set => this.RaiseAndSetIfChanged(ref _datasource, value);
        }

        #endregion

        #region BindingT

        private ConfigurazioneSettoreElencoMap _bindingT;
        public new ConfigurazioneSettoreElencoMap BindingT
        {
            get => _bindingT;
            set => this.RaiseAndSetIfChanged(ref _bindingT, value);
        }

        #endregion


    }
}
