using Configurazione.ViewModels.Map;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ViewModels;

namespace Configurazione.ViewModels
{
    public partial class SettoreInputBase : InputViewModel<ConfigurazioneSettoreMap>
    {

        protected async override Task OnSaving() { await Task.CompletedTask; }
        protected async override Task OnLoading() => await Task.CompletedTask;

        public Interaction<Unit, Unit> NomeFocus { get; } = new();
        public Interaction<Unit, Unit> LabelFocus { get; } = new();

        protected int _idDaModificare;
        protected int _idRitorno;

        
        public void SetIdDaModificare(int id)
        {
            _idDaModificare = id;
        }

        public void SetIdRitorno(int id)
        {
            _idRitorno = id;
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

        protected string Name => BindingT?.NomeSettore?.Trim() ?? string.Empty;
        protected string Label => BindingT?.EtichettaSettore?.Trim() ?? string.Empty;
        protected int CodiceSettore => BindingT?.Id ?? 0;
        protected int GetCodiceSettore => CodiceSettore;

        protected bool IsNameEmpty => string.IsNullOrWhiteSpace(Name);
        protected bool CheckLess2Name => Name.Length < 2;

        protected bool IsLabelEmpty => string.IsNullOrWhiteSpace(Label);
        protected bool CheckLess2Label => Label.Length < 2;

        protected async Task<bool> ValidaDati()
        {
            if (IsNameEmpty)
            {
                InfoLabel = "Inserire il nome del settore";
                await SetFocus(NomeFocus);
                return false;
            }
            if (CheckLess2Name)
            {
                InfoLabel = "Formato Nome Settore non valido";
                await SetFocus(NomeFocus);
                return false;
            }
            if (IsLabelEmpty)
            {
                InfoLabel = "Inserire l'etichetta del settore";
                await SetFocus(LabelFocus);
                return false;
            }
            if (CheckLess2Label)
            {
                InfoLabel = "Formato Etichetta Settore non valido";
                await SetFocus(LabelFocus);
                return false;
            }
            InfoLabel = ""; // Pulisce eventuali errori precedenti
            return true;

        }
    }

    public partial class SettoreInputBase
    {

        
        private int _codiceTipoSettore = 0;
        public int CodiceTipoSettore
        {
            get => _codiceTipoSettore;
            set => this.RaiseAndSetIfChanged(ref _codiceTipoSettore, value);
        }

        private IList<ConfigurazioneTipoSettoreMap> tipoSettoreMaps = [];
        public IList<ConfigurazioneTipoSettoreMap> TipoSettDataSource
        {
            get => tipoSettoreMaps;
            set => this.RaiseAndSetIfChanged(ref tipoSettoreMaps, value);
        }

        // 1. Aggiungi questo Subject per notificare l'esterno
        private readonly Subject<Unit> _inputEsc = new();
        public IObservable<Unit> InputEsc => _inputEsc.AsObservable();

        private readonly Subject<int> _inputBack = new();
        public IObservable<int> InputBack => _inputBack.AsObservable();


    }
}


