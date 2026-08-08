using Cassa.Core.Repository;
using Cassa.ViewModels.Map;
using ReactiveUI;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
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
        IObservable<(int postazioneId, string posizione)> PostazioneToListaSoci { get; }
        void SetPostazioneId(int postazioneId);
        void SetPosizione(string posizione);
        Task ApriScheda();
    }

    public partial class CassaPostazioneViewModel : ViewModelBase, ICassaPostazioneViewModel
    {
        // Commands
        public ReactiveCommand<Unit, Unit> EntraSocioCommand { get; }
        public ReactiveCommand<Unit, Unit> EsceSocioCommand { get; }
        public ReactiveCommand<Unit, Unit> ListaSociCommand { get; }
        public ReactiveCommand<Unit, Unit> PosizioneEnterCommand { get; }
        public ReactiveCommand<Unit, Unit> PosizioneEscCommand { get; }

        private readonly ICassaPostazioneRepository Q;
        private int _postazioneId;

        // disposables and subjects
        private readonly CompositeDisposable _disposables = new();

        protected override IObservable<bool> IsAnythingExecuting =>
            Observable.CombineLatest(new IObservable<bool>[]
            {
                base.IsAnythingExecuting ?? Observable.Return(false),

                this.WhenAnyValue(vm => vm.EntraSocioCommand)
                    .Select(cmd => cmd?.IsExecuting ?? Observable.Return(false))
                    .Switch(),

                this.WhenAnyValue(vm => vm.EsceSocioCommand)
                    .Select(cmd => cmd?.IsExecuting ?? Observable.Return(false))
                    .Switch(),

                this.WhenAnyValue(vm => vm.ListaSociCommand)
                    .Select(cmd => cmd?.IsExecuting ?? Observable.Return(false))
                    .Switch(),

                this.WhenAnyValue(vm => vm.PosizioneEnterCommand)
                    .Select(cmd => cmd?.IsExecuting ?? Observable.Return(false))
                    .Switch(),

                this.WhenAnyValue(vm => vm.PosizioneEscCommand)
                    .Select(cmd => cmd?.IsExecuting ?? Observable.Return(false))
                    .Switch()
            }, results => results.Any(x => x))
            .DistinctUntilChanged();

        public CassaPostazioneViewModel(ICassaPostazioneRepository repository) : base(null)
        {
            Q = repository ?? throw new ArgumentNullException(nameof(repository));

            EntraSocioCommand = ReactiveCommand.CreateFromTask(GoToEntraSocio);
            EsceSocioCommand = ReactiveCommand.CreateFromTask(() => Task.CompletedTask);
            ListaSociCommand = ReactiveCommand.CreateFromTask(GoToListaSoci);
            PosizioneEnterCommand = ReactiveCommand.CreateFromTask(ApriScheda);
            PosizioneEscCommand = ReactiveCommand.CreateFromTask(PosizioneEsc);

            // Subscriptions: capture thrown exceptions and dispose them with the CompositeDisposable
            _disposables.Add(EntraSocioCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Entra Socio: {ex.Message}")));
            _disposables.Add(EsceSocioCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Postazioni: {ex.Message}")));
            _disposables.Add(ListaSociCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Lista Soci: {ex.Message}")));
            _disposables.Add(PosizioneEnterCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Posizione: {ex.Message}")));
            _disposables.Add(PosizioneEscCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Esc: {ex.Message}")));

            // Ensure subjects are disposed
            _disposables.Add(_postazioneToMenu);
            _disposables.Add(_postazioneToEntraSocio);
            _disposables.Add(_postazioneToListaSoci);
        }

        protected override void OnFinalDestruction()
        {
            // Dispose of all subscriptions and subjects
            _disposables.Dispose();
            base.OnFinalDestruction();
        }

        protected override Task OnEsc()
        {
            _postazioneToMenu.OnNext(Unit.Default);
            return Task.CompletedTask;
        }

        protected Task PosizioneEsc()
        {
            IsOpen = false;
            BindingT = new CassaSchedaMap();
            return Task.CompletedTask;
        }

        protected override async Task OnLoading()
        {
            try
            {
                Titolo = "POSTAZIONE " + await Q.GetPostazioneName(_postazioneId, Token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Errore OnLoading GetPostazioneName: {ex.Message}");
                Titolo = "POSTAZIONE -";
            }

            await SetFocus(PosizioneFocus);
        }

        public void SetPostazioneId(int postazioneId) => _postazioneId = postazioneId;

        public void SetPosizione(string posizione) => BindingT.Posizione = posizione;

        private Task GoToEntraSocio()
        {
            _postazioneToEntraSocio.OnNext((_postazioneId, BindingT.Posizione));
            return Task.CompletedTask;
        }

        private Task GoToListaSoci()
        {
            _postazioneToListaSoci.OnNext((_postazioneId, BindingT.Posizione));
            return Task.CompletedTask;
        }

        public async Task ApriScheda()
        {
            if (string.IsNullOrWhiteSpace(BindingT?.Posizione))
                return;

            try
            {
                var schedaData = await Q.GetSchedaByPosizione(BindingT.Posizione, Token);
                if (schedaData == null)
                {
                    Debug.WriteLine($"No Scheda found for position: {BindingT.Posizione}");
                    BindingT = new CassaSchedaMap();
                    await SetFocus(PosizioneFocus);
                    return;
                }

                BindingT = new CassaSchedaMap(schedaData);
                IsOpen = true;
                await SetFocus(PosizioneFocus);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Errore ApriScheda: {ex.Message}");
                // optionally reset state
                BindingT = new CassaSchedaMap();
            }
        }
    }

    public partial class CassaPostazioneViewModel
    {
        private readonly Subject<Unit> _postazioneToMenu = new();
        public IObservable<Unit> PostazioneToMenu => _postazioneToMenu.AsObservable();

        
        private readonly Subject<(int postazioneId, string posizione)> _postazioneToEntraSocio = new();
        public IObservable<(int postazioneId, string posizione)> PostazioneToEntraSocio => _postazioneToEntraSocio.AsObservable();

        private readonly Subject<(int postazioneId, string posizione)> _postazioneToListaSoci = new();
        public IObservable<(int postazioneId, string posizione)> PostazioneToListaSoci => _postazioneToListaSoci.AsObservable();


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
