using AppSystem;
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
    public interface IEntraSocioViewModel : IRoutableViewModel
    {
        void SetHost(ICassaScreen host);
        void SetPostazioneId(int id);
        void SetPosizione(string numPosizione);

        IObservable<(int postazioneId, string posizione)> EntraSocioToPostazione { get; }
    }

    public partial class EntraSocioViewModel : ViewModelBase, IEntraSocioViewModel
    {
        private int _postazioneId;
        private ICassaScreen _host;
        private string _posizione;

        private IStrisciataRepository _strisciataRepository;
        private IEntraSocioRepository Q;

        public ReactiveCommand<Unit, Unit> TesseraCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> F5Command { get; private set; }
        public ReactiveCommand<Unit, Unit> PosizioneEscCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> EntraCommand { get; private set; }

        protected override IObservable<bool> IsAnythingExecuting =>
            new[]
            {
                base.IsAnythingExecuting,
                TesseraCommand?.IsExecuting ?? Observable.Return(false),
                PosizioneEscCommand?.IsExecuting ?? Observable.Return(false),
                EntraCommand?.IsExecuting ?? Observable.Return(false),
                F5Command?.IsExecuting ?? Observable.Return(false)

            }.CombineLatest(values => values.Any(x => x));

        protected IObservable<bool> CanEntra => this.WhenAnyValue(
            x => x.CanEntraLabel,
            (canEntraLabel) => string.IsNullOrEmpty(canEntraLabel)
        );


        public EntraSocioViewModel(IStrisciataRepository strisciataRepository, IEntraSocioRepository Repository) : base()
        {
            _strisciataRepository = strisciataRepository ?? throw new ArgumentNullException(nameof(strisciataRepository));
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));

            // 1. Aggiungi .Skip(1) così ignora lo stato iniziale di default (false)
            var socioFoundStream = this.WhenAnyValue(x => x.IsSocioFound)
                                       .ObserveOn(RxSchedulers.MainThreadScheduler);

            // 2. Imposta l'initialValue desiderato all'apertura della pagina
            _tesseraLabel = socioFoundStream
                                .Select(found => found ? "TESSERA :" : "TESSERA (F5) :")
                                .ToProperty(this, x => x.TesseraLabel, initialValue: "TESSERA :");

            // 3. Allinea l'initialValue vuoto per l'avvio
            _infoLabel = this.WhenAnyValue(
                        x => x.IsSocioFound,
                        x => x.IsRicercaEffettuata,
                        (found, effettuata) =>
                        {
                            // Se la ricerca è stata fatta e il socio NON è trovato
                            if (effettuata && !found)
                                return "Socio non Trovato";

                            // In tutti gli altri casi (caricamento, socio trovato, reset) la label è vuota
                            return "";
                        })
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .ToProperty(this, x => x.InfoLabel, initialValue: "");

            _canEntraLabel = this.WhenAnyValue(
                    x => x.IsSocioFound,
                    x => x.IsSocioInside, // 1. Monitoriamo anche questa nuova proprietà
                    x => x.BindingT,
                    (found, inside, bindingT) =>
                    {
                        // Caso 1: Socio non trovato
                        if (!found)
                            return "Warning Identificazione";

                        // Caso 2: Nuovo controllo - Il socio è già dentro
                        if (inside)
                            return "Socio già all'interno";

                        if (bindingT == null)
                            return "bINDINGt nULL";

                        // Caso 3: Socio trovato MA l'oggetto o la posizione sono vuoti
                        //if (bindingT == null || string.IsNullOrWhiteSpace(bindingT.Posizione))
                        //    return "Posizione Mancante";

                        // Caso 4: Tutto regolare
                        return "";
                    })
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .ToProperty(this, x => x.CanEntraLabel, initialValue: "");


            TesseraCommand = ReactiveCommand.CreateFromTask(async vm => await OnTesseraEnter());
            F5Command = ReactiveCommand.CreateFromTask(async vm => await OnF5Pressed());
            PosizioneEscCommand = ReactiveCommand.CreateFromTask(async vm => await OnPosizioneEsc());
            EntraCommand = ReactiveCommand.CreateFromTask(async vm => await OnEntra(), CanEntra);

            TesseraCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Tessera: {ex.Message}"));
            F5Command.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione F5: {ex.Message}"));
            PosizioneEscCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Posizione Esc: {ex.Message}"));
            EntraCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Entra: {ex.Message}"));
        }

        protected override void OnFinalDestruction()
        {
            // Assicuriamoci che la collezione sia nulla per il GC
            TesseraCommand = null;
            PosizioneEscCommand = null;
            F5Command = null;
            //AddTesseraCommand = DelTesseraCommand = UpdTesseraCommand = PersonSearchCommand = null;

            _strisciataRepository = null;
            Q = null;
            base.OnFinalDestruction();
        }

        protected override async Task OnLoading()
        {
            await _strisciataRepository.DevelopStrisciate(Token);
            var data  = await Q.GetIngressiByPostazione(_postazioneId, Token);
            IngressiList = [.. data.Select(data => new EntraIngressiMap(data))];
            if (IngressiList.Count > 0)
            {
                SelectedIngresso = IngressiList[0];
            }
            await SetFocus(TesseraFocus);
        }

        public void SetHost(ICassaScreen host) => _host = host;

        public void SetPostazioneId(int posizioneId) => _postazioneId = posizioneId;

        public void SetPosizione(string posizione) => _posizione = posizione;

        protected async override Task OnEsc()
        {
            _isClosing = true; // Imposta il flag per indicare che stiamo chiudendo la pagina
            _entraSocioToPostazione.OnNext((_postazioneId, _posizione)); // Notifica l'esterno
            _entraSocioToPostazione.OnCompleted(); // Completa l'osservabile per evitare memory leak    
            await Task.CompletedTask;

        }

        private async Task OnTesseraEnter()
        {
            // Stato iniziale: reset della ricerca prima di iniziare
            IsRicercaEffettuata = false;
            IsSocioFound = false;

            if (string.IsNullOrWhiteSpace(BindingT.NumeroTessera)) return;

            try
            {
                var personData = await Q.GetPersonByTessera(BindingT.NumeroTessera, Token);
                var data = new EntraSocioMap(personData);

                if (data.NumeroSocio is null)
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
                    BindingT = data;
                    Eta = BindingT.Natoil.DateIntToEta().ToString();
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
                await SetFocus(TesseraFocus);
            }
        }


        private async Task OnF5Pressed()
        {

            if (BindingT.NumeroTessera== string.Empty) return;
            IsSocioFound = true;
            BuildVirtualSocio();

            await Task.CompletedTask;

        }

        private async Task OnPosizioneEsc()
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

        private async Task OnEntra()
        {
            _isClosing = true; // Imposta il flag per indicare che stiamo chiudendo la pagina

            //if (BindingT.NumeroSocio == "0")
            //{
            //    IsSocioFound = false;
            //    _isClosing = false; // Reset del flag perché non stiamo chiudendo la pagina
            //    await SetFocus(TesseraFocus);
            //    return;
            //}
            

            try
            {
                int result = await Q.AddNewScheda(BindingT.ToDto(), Token);
                if (result == -1)
                {
                    Debug.WriteLine("Errore durante l'aggiunta della scheda.");
                    _isClosing = false; // Reset del flag perché non stiamo chiudendo la pagina
                    await SetFocus(TesseraFocus);
                    return;
                }

                _entraSocioToPostazione.OnNext((_postazioneId, _posizione));
                _entraSocioToPostazione.OnCompleted();
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Salvataggio annullato.");
                _isClosing = false;
                return;
            }
            catch (Exception ex)
            {
                _isClosing = false;
                Debug.WriteLine($"Errore: {ex.Message}");
                await SetFocus(TesseraFocus);
                return;
            }

            ; // Completa l'osservabile per evitare memory leak
        }
    }


    public partial class EntraSocioViewModel
    {
        public Interaction<Unit, Unit> TesseraFocus { get; } = new();
        public Interaction<Unit, Unit> PosizioneFocus { get; } = new();

        private readonly Subject<(int postazioneId, string posizione)> _entraSocioToPostazione = new();
        public IObservable<(int postazioneId, string posizione)> EntraSocioToPostazione => _entraSocioToPostazione.AsObservable();


        private EntraSocioMap _bindingt = new();
        public EntraSocioMap BindingT
        {
            get => this._bindingt;
            set => this.RaiseAndSetIfChanged(ref _bindingt, value);
        }

        private string _eta;
        public string Eta
        {
            get => _eta;
            set => this.RaiseAndSetIfChanged(ref _eta, value);
        }

        private bool _isSocioFound = false;
        public bool IsSocioFound
        {
            get => _isSocioFound;
            set => this.RaiseAndSetIfChanged(ref _isSocioFound, value);
        }

        private bool _isRicercaEffettuata;
        public bool IsRicercaEffettuata
        {
            get => _isRicercaEffettuata;
            set => this.RaiseAndSetIfChanged(ref _isRicercaEffettuata, value);
        }

        private bool _isSocioInside;
        public bool IsSocioInside
        {
            get => _isSocioInside;
            set => this.RaiseAndSetIfChanged(ref _isSocioInside, value);
        }


        // 2. Proprietà calcolata (OAPH) per la Label
        private readonly ObservableAsPropertyHelper<string> _tesseraLabel;
        public string TesseraLabel => _tesseraLabel.Value;

        private readonly ObservableAsPropertyHelper<string> _infoLabel;
        public string InfoLabel => _infoLabel.Value;

        private readonly ObservableAsPropertyHelper<string> _canEntraLabel;
        public string CanEntraLabel => _canEntraLabel.Value;

        private List<EntraIngressiMap> _ingressiList = [];
        public List<EntraIngressiMap> IngressiList
        {
            get => _ingressiList;
            set => this.RaiseAndSetIfChanged(ref _ingressiList, value);
        }

        private EntraIngressiMap _selectedIngressi;
        public EntraIngressiMap SelectedIngresso
        {
            get => _selectedIngressi;
            set => this.RaiseAndSetIfChanged(ref _selectedIngressi, value);

        }

        //private string infolabel = string.Empty;
        //public string InfoLabel
        //{
        //    get => infolabel;
        //    set => this.RaiseAndSetIfChanged(ref infolabel, value);
        //}
    }
}
