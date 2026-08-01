using Configurazione.ViewModels.Map;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ViewModels;

namespace Configurazione.ViewModels
{
    public partial class PostazioneInputBase : InputViewModel<ConfigurazionePostazioneMap>
    {

        protected string Name => BindingT?.NomePostazione?.Trim() ?? string.Empty;
        int CodicePostazione => BindingT?.Id ?? 0;
        protected int GetCodicePostazione => CodicePostazione;

        protected bool IsNameEmpty => string.IsNullOrWhiteSpace(Name);
        protected bool CheckLess2Name => Name.Length < 2;

        public Interaction<Unit, Unit> NomeFocus { get; } = new();

        protected int _idDaModificare;
        protected int _idRitorno;

        protected async override Task OnSaving() { await Task.CompletedTask; }
        protected async override Task OnLoading() => await Task.CompletedTask;

        public PostazioneInputBase() : base()
        {
            this.WhenActivated(d =>
            {

                // Logica IsCassa: aggiorna RientroVisibile quando cambia il tipo postazione
                this.WhenAnyValue(
                    x => x.BindingT,
                    x => x.BindingT.CodiceTipoPostazione,
                    (bt, codice) => bt is not null && codice == 2) // Questa è la tua logica IsCassa
                .Subscribe(isCassa =>
                {
                    // Se è una cassa, il rientro è visibile (o invisibile, a seconda della tua logica)
                    RientroVisibile = isCassa;
                })
                .DisposeWith(d);

            });
        }
        
        public void SetIdDaModificare(int id)
        {
            _idDaModificare = id;
        }

        public void SetIdRitorno(int id)
        {
            _idRitorno = id;
        }

        protected async Task<bool> ValidaDati()
        {
            if (IsNameEmpty)
            {
                InfoLabel = "Inserire il nome della posizione";
                await SetFocus(NomeFocus);
                return false;
            }

            if (CheckLess2Name)
            {
                InfoLabel = "Formato Nome Postazione non valido";
                await SetFocus(NomeFocus);
                return false;
            }

            InfoLabel = ""; // Pulisce eventuali errori precedenti
            return true;
        }

        protected async override Task OnEsc()
        {
            if (_isClosing) return; // Protezione contro il multi-ESC

            await SetFocus(EscFocus, 0);
            _isClosing = true; // "Congeliamo" prima di uscire

            _inputEsc.OnNext(Unit.Default); // Notifica l'esterno che ESC è stato premuto
            _inputEsc.OnCompleted(); // Completa l'osservabile per evitare memory leak e notificare che non ci saranno più eventi

        }

        protected async Task OnBack(int value = 0)
        {
            _isClosing = true;

            _inputBack.OnNext(value); // Notifica l'esterno che Back è stato premuto con il valore specificato
            _inputBack.OnCompleted();
            await Task.CompletedTask;

        }
    }

    public partial class PostazioneInputBase
    {
  
        private bool _rientroVisibile = true;
        public bool RientroVisibile
        {
            get => _rientroVisibile;
            set => this.RaiseAndSetIfChanged(ref _rientroVisibile, value);
        }

        private IList<ConfigurazioneTipoPostazioneMap> tipoPostazioneMaps = [];
        public IList<ConfigurazioneTipoPostazioneMap> TipoPostDataSource
        {
            get => tipoPostazioneMaps;
            set => this.RaiseAndSetIfChanged(ref tipoPostazioneMaps, value);
        }

        private IList<ConfigurazioneTipoRientroMap> _tipoRientroDataSource = [];
        public IList<ConfigurazioneTipoRientroMap> TipoRientroDataSource
        {
            get => _tipoRientroDataSource;
            set => this.RaiseAndSetIfChanged(ref _tipoRientroDataSource, value);
        }

        // 1. Aggiungi questo Subject per notificare l'esterno
        private readonly Subject<Unit> _inputEsc = new();
        public IObservable<Unit> InputEsc => _inputEsc.AsObservable();

        private readonly Subject<int> _inputBack = new();
        public IObservable<int> InputBack => _inputBack.AsObservable();

    }
}
