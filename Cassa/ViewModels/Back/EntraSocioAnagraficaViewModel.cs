using AppSystem;
using Cassa.Core.Repository;
using Cassa.ViewModels.Map;
using ReactiveUI;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
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

            var socioFoundStream = this.WhenAnyValue(x => x.IsSocioFound, x => x.IsRicercaEffettuata)
                               .ObserveOn(RxSchedulers.MainThreadScheduler)
                               .Skip(1);
                               

            TesseraLabel = IsRicercaEffettuata
                ? (IsSocioFound ? "TESSERA: " : "TESSERA (F5):")
                : "TESSERA :";

            _disposables.Add(socioFoundStream.Subscribe(tuple =>
            {
                var (isFound, isRicerca) = tuple;

                if (!isRicerca)
                {
                    TesseraLabel = "TESSERA :";
                    return;
                }

                TesseraLabel = isFound ? "TESSERA :" : "TESSERA (F5) :";
            }));



        }

        protected override void OnFinalDestruction()
        {
            // Dispose of all subscriptions and subjects
            _disposables.Dispose();
            base.OnFinalDestruction();
        }

        protected override async Task OnLoading()
        {
            InfoLabel = string.Empty;
            await SetFocus(TesseraFocus);
        }

        private async Task TesseraAsync()
        {
            // Stato iniziale: reset della ricerca prima di iniziare
            IsRicercaEffettuata = false;
            IsSocioFound = false;
            IsSocioInside = false;

            if (string.IsNullOrWhiteSpace(BindingT.NumeroTessera)) return;

            try
            {
                var personData = await Q.GetPersonByTessera(BindingT.NumeroTessera, Token);
                var data = new EntraSocioMap(personData);

                if (string.IsNullOrWhiteSpace(data.NumeroSocio))
                {
                    // NOTA: Rimane false per attivare la InfoLabel "Socio non Trovato" 
                    // grazie alla logica (effettuata && !found) se decidi di metterla a true qui.
                    // Per coerenza con la tua InfoLabel, la ricerca DEVE essere considerata effettuata.
                    IsSocioFound = false;
                    IsRicercaEffettuata = true;

                    string tesseraCorrente = BindingT.NumeroTessera;

                    // Al fine di evitare che _canEntraLabel mostri "Posizione Mancante" 
                    // sovrascrivendo "Socio non Trovato", azzeriamo la posizione o gestiamo l'oggetto.
                    BindingT = new EntraSocioMap
                    {
                        NumeroTessera = tesseraCorrente,
                        Posizione = null // Verrà intercettato da !IsSocioFound dando la precedenza a "Warning Identificazione"
                    };
                    Eta = string.Empty;
                }
                else
                {
                    IsSocioFound = true;
                    IsRicercaEffettuata = true;
                    IsSocioInside = await Q.EsisteSocioInside(data.ToDto(), Token);
                    //posizioniEsistentiHash = new HashSet<string>(
                    //                    await Q.GetPosizioniAsync(),
                    //                    StringComparer.OrdinalIgnoreCase);
                    BindingT = data;
                    Eta = BindingT.Natoil.DateIntToEta().ToString();

                    if (!IsSocioInside) await SetFocus(PosizioneFocus);

                    
                }
            }
            catch (Exception ex)
            {
                IsSocioFound = false;
                IsRicercaEffettuata = false; // La ricerca è fallita tecnicamente, non è "Socio non trovato"
                Debug.WriteLine($"Errore durante la ricerca del socio: {ex.Message}");
            }
            finally
            {
                // Sposta il focus alla fine del ciclo di rendering
                //await SetFocus(TesseraFocus);
            }
        }

        

        private async Task F5Async()
        {
            // Logica per il comando F5
            await Task.CompletedTask;
        }

        private async Task PosizioneEscAsync()
        {
            BindingT = new(); // Resetta i dati
            Eta = string.Empty;
            IsSocioFound = false;
            IsRicercaEffettuata = false;
            await SetFocus(TesseraFocus);
        }

        private void BuildVirtualSocio()
        {
            BindingT.Cognome = "Socio";
            BindingT.Nome = "Virtuale";
            BindingT.NumeroSocio = "-" + BindingT.NumeroTessera;
            Eta = string.Empty;
            BindingT.CodiceSocio = -1; // Indica che è un socio virtuale

            // Qui puoi fare ulteriori operazioni con virtualSocio, come salvarlo o passarlo ad altri componenti
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

        private string _eta;
        public string Eta
        {
            get => _eta;
            set => this.RaiseAndSetIfChanged(ref _eta, value);
        }

        private string _infoLabel = string.Empty;
        public string InfoLabel
        {
            get => _infoLabel;
            private set => this.RaiseAndSetIfChanged(ref _infoLabel, value);
        }


        private bool _isSocioFoundFlag;
        public bool IsSocioFound
        {
            get => _isSocioFoundFlag;
            private set => this.RaiseAndSetIfChanged(ref _isSocioFoundFlag, value);
        }

        private bool _isRicercaEffettuata;
        public bool IsRicercaEffettuata
        {
            get => _isRicercaEffettuata;
            private set => this.RaiseAndSetIfChanged(ref _isRicercaEffettuata, value);
        }


        private bool _isSocioInside;
        public bool IsSocioInside
        {
            get => _isSocioInside;
            private set => this.RaiseAndSetIfChanged(ref _isSocioInside, value);
        }

        

        public Interaction<Unit, Unit> TesseraFocus { get; } = new();
        public Interaction<Unit, Unit> PosizioneFocus { get; } = new();
    }
}
