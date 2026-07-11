using Cassa.ViewModels.Map;
using ReactiveUI;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ViewModels;

namespace Cassa.ViewModels
{
    public interface ICassaPostazioneViewModel : IRoutableViewModel
    {
        // Define any properties or methods that the CassaPostazioneViewModel should implement
        IObservable<Unit> PostazioneToMenu { get; }
    }

    public partial class CassaPostazioneViewModel : ViewModelBase, ICassaPostazioneViewModel
    {
        public ReactiveCommand<Unit, Unit> EntraSocioCommand { get; }
        public ReactiveCommand<Unit, Unit> EsceSocioCommand { get; }
        public ReactiveCommand<Unit, Unit> ListaSociCommand { get; }
        public ReactiveCommand<Unit, Unit> PosizioneEnterCommand { get; }

        private readonly ObservableAsPropertyHelper<bool> _isOpen;
        public bool IsOpen => _isOpen.Value;


        protected override IObservable<bool> IsAnythingExecuting =>
        _isOpen = Observable.CombineLatest(
            // 1. Comandi ereditati dalla classe base
            this.WhenAnyObservable(x => x.LoadCommand.IsExecuting).StartWith(false),
            this.WhenAnyObservable(x => x.SaveCommand.IsExecuting).StartWith(false),
            this.WhenAnyObservable(x => x.EscPressedCommand.IsExecuting).StartWith(false),
            // 2. Comandi specifici osservati in modo sicuro direttamente tramite le loro proprietà
            this.WhenAnyObservable(
                x => x.EntraSocioCommand.IsExecuting,
                x => x.EsceSocioCommand.IsExecuting,
                x => x.ListaSociCommand.IsExecuting,
                x => x.PosizioneEnterCommand.IsExecuting

            ).StartWith(false),
            // Se anche uno solo è in esecuzione, restituisce true
            (baseLoad, baseSave, baseEsc, localExec) => baseLoad || baseSave || baseEsc || localExec)
        .DistinctUntilChanged();

        public CassaPostazioneViewModel() : base(null)
        {
            EntraSocioCommand = ReactiveCommand.CreateFromTask(GoToEntraSocio);
            EsceSocioCommand = ReactiveCommand.CreateFromTask(() => Task.CompletedTask); // Placeholder for actual logic
            ListaSociCommand = ReactiveCommand.CreateFromTask(() => Task.CompletedTask); // Placeholder for actual logic
            PosizioneEnterCommand = ReactiveCommand.CreateFromTask(() => Task.CompletedTask); // Placeholder for actual logic

            EntraSocioCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Entra Socio: {ex.Message}"));
            EsceSocioCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Postazioni: {ex.Message}"));
            ListaSociCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Lista Soci: {ex.Message}"));
            PosizioneEnterCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Posizione: {ex.Message}"));
        }

        protected override void OnFinalDestruction()
        {
            // Assicuriamoci che la collezione sia nulla per il GC
            //Q = null;
            base.OnFinalDestruction();
        }

        protected override Task OnEsc()
        {
            // Logic for handling the "Esc" command
            _postazioneToMenu.OnNext(Unit.Default);
            return Task.CompletedTask;
        }

        private async Task GoToEntraSocio()
        {
            // Logic for navigating to the "Entra Socio" view
            
            await Task.CompletedTask;
        }
    }

    public partial class CassaPostazioneViewModel
    {
        private readonly Subject<Unit> _postazioneToMenu = new();
        public IObservable<Unit> PostazioneToMenu => _postazioneToMenu.AsObservable();

    }

    public partial class CassaPostazioneViewModel
    {
        private string _titolo = string.Empty;
        public string Titolo
        {
            get => _titolo;
            set => this.RaiseAndSetIfChanged(ref _titolo, value);
        }

        private CassaSchedaMap bindingt = new();
        public CassaSchedaMap BindingT
        {
            get => bindingt;
            set => this.RaiseAndSetIfChanged(ref bindingt, value);

        }

        public Interaction<Unit, Unit> PosizioneFocus { get; } = new();

        

        private readonly Subject<bool> _isOpenManualTrigger = new();

    }
}
