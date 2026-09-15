using AppSystem;
using Cassa.Core.Repository;
using Cassa.ViewModels.Map;
using ReactiveUI;
using Splat;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
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

        private HashSet<string> posizioniEsistentiHash;

        private readonly CompositeDisposable _disposables = new();

        public ReactiveCommand<Unit, Unit> EntraCommand { get; private set; }

        protected override IObservable<bool> IsAnythingExecuting =>
            new[]
            {
                base.IsAnythingExecuting,
                EntraCommand?.IsExecuting ?? Observable.Return(false)

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

            _isPosizioneEsistente = this.WhenAnyValue(x => x.BindingT.Posizione)
                .Throttle(TimeSpan.FromMilliseconds(300)) // Evita sovraccarichi durante la digitazione veloce
                .Select(posizione =>
                {
                    if (string.IsNullOrWhiteSpace(posizione))
                        return false;

                    // Ricerca immediata O(1) nell'HashSet
                    return posizioniEsistentiHash.Contains(posizione);
                })
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .ToProperty(this, x => x.IsPosizioneEsistente, initialValue: false);

            // Subscribe to ErrorText changes and call handler (skip initial emission if undesiderata)
            this.WhenAnyValue(vm => vm.ErrorText)
                .DistinctUntilChanged()
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(errorText => OnErrorTextChanged(errorText))
                .DisposeWith(_disposables);

            _canEntraLabel = this.WhenAnyValue(
                    x => x.IsSocioFound,
                    x => x.IsSocioInside,
                    x => x.BindingT,             // Monitoriamo l'oggetto per evitare NullReference
                    x => x.BindingT.Posizione,    // Monitoriamo la stringa digitata
                    x => x.IsPosizioneEsistente,  // 2. Monitoriamo lo stato della verifica in memoria
                    (found, inside, bindingT, posizione, esiste) =>
                    {
                        // Caso 1: Socio non trovato
                        if (!found)
                            return "Warning Identificazione";

                        // Caso 2: Il socio è già dentro
                        if (inside)
                            return "Socio già all'interno";

                        // Caso 3: L'oggetto o la stringa sono vuoti
                        if (bindingT == null || string.IsNullOrWhiteSpace(posizione))
                            return "Posizione Mancante";

                        // Caso 4: Nuovo controllo - La posizione digitata non è censita nel DB
                        if (esiste)
                            return "Posizione Già Occupata";

                        // Caso 5: Tutto regolare
                        return "";
                    })
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .ToProperty(this, x => x.CanEntraLabel, initialValue: "");

            

            EntraCommand = ReactiveCommand.CreateFromTask(async vm => await OnEntra(), CanEntra);

            _disposables.Add(EntraCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Entra: {ex.Message}")));
        }

        protected override void OnFinalDestruction()
        {
            // Assicuriamoci che la collezione sia nulla per il GC
            _disposables.Dispose(); // Dispose di tutte le sottoscrizioni
            //AddTesseraCommand = DelTesseraCommand = UpdTesseraCommand = PersonSearchCommand = null;

            _strisciataRepository = null;
            Q = null;
            base.OnFinalDestruction();
        }

        protected override async Task OnLoading()
        {
            await _strisciataRepository.DevelopStrisciate(Token);
            var data  = await Q.GetIngressiByPostazione(_postazioneId, Token);
            
            if (data.Count == 0)
            {
                
                ErrorText = "Nessun ingresso disponibile per questa postazione.";
                IsAnagraficaEnabled = false; // Blocca l'inserimento dei dati
                return;
            }

            IngressiList = [.. data.Select(data => new EntraIngressiMap(data))];
            if (IngressiList.Count > 0)
            {
                SelectedIngresso = IngressiList[0];
            }

            //await SetFocus(TesseraFocus);
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

        

        private void OnErrorTextChanged(string errorText)
        {
            // comportamento predefinito: log dell'errore
            if (!string.IsNullOrWhiteSpace(errorText))
            {
                Debug.WriteLine($"Errore cambiato: {errorText}");
            }

            ErrorText = errorText; // Aggiorna la proprietà ErrorText se necessario
        }


        private async Task OnF5Pressed()
        {

            if (BindingT.NumeroTessera== string.Empty) return;
            IsSocioFound = true;
            //BuildVirtualSocio();

            await Task.CompletedTask;

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

                int result = await Q.AddNewScheda(BindingT.ToDto(), SelectedIngresso.ToDto(),Token);

                

                if (result == -1)
                {
                    Debug.WriteLine("Errore durante l'aggiunta della scheda.");
                    _isClosing = false; // Reset del flag perché non stiamo chiudendo la pagina
                    //await SetFocus(TesseraFocus);
                    return;
                }


                _posizione = BindingT.Posizione; // Salva la posizione corrente prima di chiudere
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
                //await SetFocus(TesseraFocus);
                return;
            }

            ; // Completa l'osservabile per evitare memory leak
        }
    }


    public partial class EntraSocioViewModel
    {
        //public Interaction<Unit, Unit> TesseraFocus { get; } = new();
        public Interaction<Unit, Unit> PosizioneFocus { get; } = new();

        private readonly Subject<(int postazioneId, string posizione)> _entraSocioToPostazione = new();
        public IObservable<(int postazioneId, string posizione)> EntraSocioToPostazione => _entraSocioToPostazione.AsObservable();


        private EntraSocioMap _bindingt = new();
        public EntraSocioMap BindingT
        {
            get => this._bindingt;
            set => this.RaiseAndSetIfChanged(ref _bindingt, value);
        }

        private IEntraSocioAnagraficaViewModel _anagraficaViewModel = Locator.Current.GetService<IEntraSocioAnagraficaViewModel>();
        public IEntraSocioAnagraficaViewModel AnagraficaViewModel
        {
            get => _anagraficaViewModel;
            set => this.RaiseAndSetIfChanged(ref _anagraficaViewModel, value);
        }

        private bool isAnagraficaEnabled = true;
        public bool IsAnagraficaEnabled
        {
            get => isAnagraficaEnabled;
            set => this.RaiseAndSetIfChanged(ref isAnagraficaEnabled, value);
        }

        private string _errorText = string.Empty;
        public string ErrorText
        {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);

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

        private readonly ObservableAsPropertyHelper<bool> _isPosizioneEsistente;
        // Proprietà di sola lettura alimentata dal flusso reattivo
        public bool IsPosizioneEsistente => _isPosizioneEsistente.Value;

        

        private readonly ObservableAsPropertyHelper<string> _infoLabel;
        public string InfoLabel => _infoLabel.Value;


        private bool _isEntryFormBlocked;
        // Proprietà di sola lettura alimentata dal flusso reattivo
        public bool IsEntryFormBlocked
        {
            get => _isEntryFormBlocked;
            set => this.RaiseAndSetIfChanged(ref _isEntryFormBlocked, value);
        }



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

        
    }
}
