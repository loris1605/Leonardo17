using Cassa.Core.Repository;
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
        IObservable<(int postazioneId, string posizione)> PostazioneToEntraSocio { get; }
        void SetPostazioneId(int postazioneId);
        void SetPosizione(string posizione);
    }

    public partial class CassaPostazioneViewModel : ViewModelBase, ICassaPostazioneViewModel
    {
        public ReactiveCommand<Unit, Unit> EntraSocioCommand { get; }
        public ReactiveCommand<Unit, Unit> EsceSocioCommand { get; }
        public ReactiveCommand<Unit, Unit> ListaSociCommand { get; }
        public ReactiveCommand<Unit, Unit> PosizioneEnterCommand { get; }

        private readonly ICassaPostazioneRepository Q;
        private int _postazioneId;

        protected override IObservable<bool> IsAnythingExecuting =>
            new[]
            {
                base.IsAnythingExecuting,
                EntraSocioCommand?.IsExecuting ?? Observable.Return(false),
                EsceSocioCommand?.IsExecuting ?? Observable.Return(false),
                ListaSociCommand?.IsExecuting ?? Observable.Return(false),
                PosizioneEnterCommand?.IsExecuting ?? Observable.Return(false)
            }.CombineLatest(values => values.Any(x => x));



        public CassaPostazioneViewModel(ICassaPostazioneRepository Repository) : base(null)
        {
           Q = Repository ?? throw new ArgumentNullException(nameof(Repository));

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

        protected override async Task OnLoading()
        {
            Titolo = "POSTAZIONE " + await Q.GetPostazioneName(_postazioneId, Token);
            await SetFocus(PosizioneFocus);
            await Task.CompletedTask;
        }

        public void SetPostazioneId(int postazioneId)
        {
            _postazioneId = postazioneId;
        }

        public void SetPosizione(string posizione)
        {
            BindingT.Posizione = posizione;
        }

        private Task GoToEntraSocio()
        {
            // Logic for navigating to the "Entra Socio" view
            _postazioneToEntraSocio.OnNext((_postazioneId, BindingT.Posizione));
            return Task.CompletedTask;
        }
    }

    public partial class CassaPostazioneViewModel
    {
        private readonly Subject<Unit> _postazioneToMenu = new();
        public IObservable<Unit> PostazioneToMenu => _postazioneToMenu.AsObservable();

        
        private readonly Subject<(int postazioneId, string posizione)> _postazioneToEntraSocio = new();
        public IObservable<(int postazioneId, string posizione)> PostazioneToEntraSocio => _postazioneToEntraSocio.AsObservable();

        private bool _isOpen = false;
        public bool IsOpen
        {
            get => _isOpen;
            set => this.RaiseAndSetIfChanged(ref _isOpen, value);
        }
        private readonly Subject<bool> _isOpenManualTrigger = new();

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

        
    }
}
