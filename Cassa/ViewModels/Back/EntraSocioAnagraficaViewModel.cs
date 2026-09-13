using Cassa.Core.Repository;
using Cassa.ViewModels.Map;
using ReactiveUI;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using ViewModels;

namespace Cassa.ViewModels
{
    public interface IEntraSocioAnagraficaViewModel : IRoutableViewModel
    {
        
    }


    public partial class EntraSocioAnagraficaViewModel : ViewModelBase, IEntraSocioAnagraficaViewModel
    {

        // ---------------------------------------------------------------------
        // 1. Dipendenze e Campi Privati
        // ---------------------------------------------------------------------
        private readonly IEntraSocioRepository Q;
        private readonly CompositeDisposable _disposables = new();

        // Commands
        public ReactiveCommand<Unit, Unit> TesseraCommand { get; }
        public ReactiveCommand<Unit, Unit> F5Command { get; }
        public ReactiveCommand<Unit, Unit> PosizioneEscCommand { get; }

        // Freeze after the first execution to prevent multiple simultaneous executions
        protected override IObservable<bool> IsAnythingExecuting =>
            new[]
            {
                base.IsAnythingExecuting,
                TesseraCommand?.IsExecuting ?? Observable.Return(false),
                PosizioneEscCommand?.IsExecuting ?? Observable.Return(false),
                F5Command?.IsExecuting ?? Observable.Return(false)

            }.CombineLatest(values => values.Any(x => x));

        public EntraSocioAnagraficaViewModel(IEntraSocioRepository Repository)
        {
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));

            // Inizializzazione dei comandi
            TesseraCommand = ReactiveCommand.CreateFromTask(TesseraAsync);
            F5Command = ReactiveCommand.CreateFromTask(F5Async);
            PosizioneEscCommand = ReactiveCommand.CreateFromTask(PosizioneEscAsync);

            _disposables.Add(TesseraCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Tessera: {ex.Message}")));
            _disposables.Add(F5Command.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione F5: {ex.Message}")));
            _disposables.Add(PosizioneEscCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Posizione Esc: {ex.Message}")));

            // Gestione delle disposizioni
            _disposables.Add(TesseraCommand);
            _disposables.Add(F5Command);
            _disposables.Add(PosizioneEscCommand);


             // Inizializza prima _isSocioFound per evitare NullReference quando si osserva IsSocioFound
            _isSocioFound = this.WhenAnyValue(x => x.BindingT.NumeroTessera)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .DistinctUntilChanged()
                .SelectMany(numero =>
                {
                    if (string.IsNullOrWhiteSpace(numero))
                        return Observable.Return(false);

                    // Cattura snapshot del DTO per la chiamata asincrona
                    var dto = BindingT.ToDto();
                    return Observable.FromAsync(ct => Q.EsisteSocioInside(dto, ct))
                                     .Catch<bool, Exception>(ex =>
                                     {
                                         Debug.WriteLine($"Errore verifica socio inside: {ex.Message}");
                                         return Observable.Return(false);
                                     });
                })
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .ToProperty(this, x => x.IsSocioFound, initialValue: true);




            // 1. Aggiungi .Skip(1) così ignora lo stato iniziale di default (false)
            var socioFoundStream = this.WhenAnyValue(x => x.IsSocioFound)
                                       .ObserveOn(RxSchedulers.MainThreadScheduler);

                        

        }

        protected override void OnFinalDestruction()
        {
            // Dispose of all subscriptions and subjects
            _disposables.Dispose();
            base.OnFinalDestruction();
        }

        protected override async Task OnLoading()
        {
            await SetFocus(TesseraFocus);
        }

        private async Task TesseraAsync()
        {
            // Logica per il comando Tessera
            await Task.CompletedTask;
        }

        private async Task F5Async()
        {
            // Logica per il comando F5
            await Task.CompletedTask;
        }

        private async Task PosizioneEscAsync()
        {
            // Logica per il comando PosizioneEsc
            await Task.CompletedTask;

        }
    }

    public partial class EntraSocioAnagraficaViewModel
    {
        private EntraSocioMap _bindingt = new();
        public EntraSocioMap BindingT
        {
            get => this._bindingt;
            set => this.RaiseAndSetIfChanged(ref _bindingt, value);
        }

        private string _tesseraLabel = "TESSERA :";
        public string TesseraLabel
        {
            get => _tesseraLabel;
            private set => this.RaiseAndSetIfChanged(ref _tesseraLabel, value);
        }



        private readonly ObservableAsPropertyHelper<bool> _isSocioFound;
        public bool IsSocioFound => _isSocioFound.Value;



        public Interaction<Unit, Unit> TesseraFocus { get; } = new();
    }
}
