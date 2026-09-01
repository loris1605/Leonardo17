using Configurazione.ViewModels.Map;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ViewModels;

namespace Configurazione.ViewModels
{
    public partial class TipoRientroInputBase : InputViewModel<ConfigurazioneTipoRientroMap>
    {
        protected async override Task OnSaving() { await Task.CompletedTask; }
        protected async override Task OnLoading() => await Task.CompletedTask;
        public Interaction<Unit, Unit> NomeFocus { get; } = new();
        
        protected int _idDaModificare;
        protected int _idRitorno;
        public string Name => BindingT.Nome.Trim() is null ? "" : BindingT.Nome.Trim();
        protected int GetCodiceTipoRientro => BindingT is null ? 0 : BindingT.Id;
        protected bool IsNameEmpty => BindingT is not null && (Name == "");
        protected bool CheckLess2Name => Name.Length < 2;

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

    }

    public partial class TipoRientroInputBase
    {
        protected async Task<bool> ValidaDati()
        {
            if (IsNameEmpty)
            {
                InfoLabel = "Inserire il nome del Tipo Rientro";
                await SetFocus(NomeFocus);
                return false;
            }
            if (CheckLess2Name)
            {
                InfoLabel = "Formato Nome Tipo Rientro non valido";
                await SetFocus(NomeFocus);
                return false;
            }
            
            InfoLabel = ""; // Pulisce eventuali errori precedenti
            return true;

        }

        // 1. Aggiungi questo Subject per notificare l'esterno
        private readonly Subject<Unit> _inputEsc = new();
        public IObservable<Unit> InputEsc => _inputEsc.AsObservable();

        private readonly Subject<int> _inputBack = new();
        public IObservable<int> InputBack => _inputBack.AsObservable();
    }


}
