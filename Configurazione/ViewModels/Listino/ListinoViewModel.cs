using Configurazione.Core.Repository;
using Configurazione.ViewModels.Map;
using ReactiveUI;
using System.Diagnostics;

namespace Configurazione.ViewModels
{
    public interface IListinoViewModel : IConfigurazioneCrudViewModel { }

    public partial class ListinoViewModel(IConfigurazioneSettoreRepository settoreRepository, 
                                          IConfigurazioneListinoRepository listinoRepository) : SettoreInputBase(), IListinoViewModel
    {
        private IConfigurazioneListinoRepository Q = listinoRepository ?? throw new ArgumentNullException(nameof(listinoRepository));
        private IConfigurazioneSettoreRepository sQ = settoreRepository ?? throw new ArgumentNullException(nameof(settoreRepository));

        protected override void OnFinalDestruction()
        {
            Q = null;
            sQ = null;
            base.OnFinalDestruction();
        }

        protected override async Task OnLoading()
        {
            var settore = await sQ.FirstSettore(_idDaModificare, Token);
            Titolo = $"Listini per il settore: {settore?.NomeSettore ?? "Sconosciuto"}";
            var data = await Q.GetListini(_idDaModificare, Token);

            DataSource = [.. data.Select(dto => new ConfigurazioneTariffaMap(dto))];
            await SetFocus(EscFocus);
        }

        protected async override Task OnSaving()
        {
            if (DataSource == null) return;
            _isClosing = true;

            var dtoSource = DataSource.Select(p => p.ToDto()).ToList();

            try
            {
                InfoLabel = "Salvataggio listini...";

                if (!await Q.SaveListini(_idDaModificare, dtoSource, Token))
                {
                    InfoLabel = "Errore Database: modifica listini fallita";
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

    public partial class ListinoViewModel
    {
        #region DataSource

        private IList<ConfigurazioneTariffaMap> _datasource = [];
        public IList<ConfigurazioneTariffaMap> DataSource
        {
            get => _datasource;
            set => this.RaiseAndSetIfChanged(ref _datasource, value);
        }

        #endregion

        #region BindingT

        private ConfigurazioneTariffaMap _bindingT;
        public new ConfigurazioneTariffaMap BindingT
        {
            get => _bindingT;
            set => this.RaiseAndSetIfChanged(ref _bindingT, value);
        }

        #endregion


    }
}
