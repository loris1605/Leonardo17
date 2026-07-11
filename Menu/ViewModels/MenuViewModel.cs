using Contracts;
using DTO.Repository;
using Menu.ViewModels.Map;
using Models.Entity.Global;
using ReactiveUI;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ViewModels;

namespace Menu.ViewModels
{
   
    public partial class MenuViewModel : ViewModelBase, IMenuViewModel
    {
        // ---------------------------------------------------------------------
        // 1. Dipendenze e Campi Privati
        // ---------------------------------------------------------------------
        private IMenuRepository Q;
        private IScreen _host;

        // Implementazione dell'interfaccia IRoutableViewModel richiesta da ReactiveUI
        public new IScreen HostScreen => _host;

        // Gestore per la pulizia dei flussi OAPH ed evitare Memory Leak al GC
        private readonly CompositeDisposable _menuDisposables = [];

        // ---------------------------------------------------------------------
        // 2. Comandi Reattivi Esposti alla View
        // ---------------------------------------------------------------------
        public ReactiveCommand<string, Unit> NavigateCommand { get; }
        public ReactiveCommand<int, Unit> CassaPostazioneCommand { get; }
        public ReactiveCommand<Unit, Unit> LogoutCommand { get; }
        public ReactiveCommand<Unit, Unit> ConnectionCommand { get; }
        public ReactiveCommand<Unit, Unit> ConfigurazioneCommand { get; }
        public ReactiveCommand<Unit, Unit> SociCommand { get; }
        public ReactiveCommand<Unit, Unit> ApriGiornataCommand { get; }
        

        // ---------------------------------------------------------------------
        // 3. Flussi Reattivi Centralizzati (Override Controllo Doppio Clic Senza "base")
        // ---------------------------------------------------------------------
        protected override IObservable<bool> IsAnythingExecuting =>
        Observable.CombineLatest(
            // 1. Comandi ereditati dalla classe base
            this.WhenAnyObservable(x => x.LoadCommand.IsExecuting).StartWith(false),
            this.WhenAnyObservable(x => x.SaveCommand.IsExecuting).StartWith(false),
            this.WhenAnyObservable(x => x.EscPressedCommand.IsExecuting).StartWith(false),
            // 2. Comandi specifici osservati in modo sicuro direttamente tramite le loro proprietà
            this.WhenAnyObservable(
                x => x.CassaPostazioneCommand.IsExecuting,
                x => x.LogoutCommand.IsExecuting,
                x => x.ConnectionCommand.IsExecuting,
                x => x.ConfigurazioneCommand.IsExecuting,
                x => x.SociCommand.IsExecuting,
                x => x.ApriGiornataCommand.IsExecuting
            ).StartWith(false),
            // Se anche uno solo è in esecuzione, restituisce true
            (baseLoad, baseSave, baseEsc, localExec) => baseLoad || baseSave || baseEsc || localExec)
        .DistinctUntilChanged();


        // ---------------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------------
        public MenuViewModel(IScreen host, IMenuRepository Repository) : base(null)
        {
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
            _host = host;

            // 1. Collegamento e aggiornamento delle proprietà OAPH definite nel file parziale
            var chiudiGiornataObs = this.WhenAnyValue(x => x.ApriGiornataEnabled)
                .Select(x => !x)
                .ObserveOn(RxSchedulers.MainThreadScheduler);

            _chiudiGiornataEnabled = chiudiGiornataObs.ToProperty(this, x => x.ChiudiGiornataEnabled);
            chiudiGiornataObs.Subscribe().DisposeWith(_menuDisposables);

            var sessioneContabileObs = this.WhenAnyValue(x => x.ApriGiornataEnabled)
            .Select(v => $"Sessione Contabile {(v ? "Chiusa" : "Aperta")}")
            .ObserveOn(RxSchedulers.MainThreadScheduler);

            _sessioneContabile = sessioneContabileObs.ToProperty(this, x => x.SessioneContabile);
            sessioneContabileObs.Subscribe().DisposeWith(_menuDisposables);

            // Vincolo generale di navigazione basato sullo stato globale IsLoading della base
            var canNavigate = this.WhenAnyValue(x => x.IsLoading)
                .Select(isLoading => !isLoading)
                .ObserveOn(RxSchedulers.MainThreadScheduler);

            var canApriFinal = this.WhenAnyValue(x => x.ApriGiornataEnabled);

            // Inizializzazione dei Comandi
            CassaPostazioneCommand = ReactiveCommand.CreateFromTask<int>(GoToCassa, canNavigate);
            LogoutCommand = ReactiveCommand.CreateFromTask(() => GoTo(_menuToLogin), canNavigate);
            ConnectionCommand = ReactiveCommand.CreateFromTask(() => GoTo(_menuToConnection), canNavigate);
            ConfigurazioneCommand = ReactiveCommand.CreateFromTask(() => GoTo(_menuToConfigurazione), canNavigate);
            SociCommand = ReactiveCommand.CreateFromTask(() => GoTo(_menuToSoci), canNavigate);
            ApriGiornataCommand = ReactiveCommand.CreateFromTask(ExecuteOpenGiornata, canApriFinal);
            //CassaCommand = ReactiveCommand.CreateFromTask(() => GoToCassa(SelectedPostazione?.IDPOSTAZIONE ?? 0), canNavigate);

            //4.Gestione centralizzata delle Eccezioni(Ciclo di vita del ViewModel)
            CassaPostazioneCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Cassa: {ex.Message}"));
            LogoutCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Logout: {ex.Message}"));
            ConnectionCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Connessione: {ex.Message}"));
            ConfigurazioneCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Configurazione: {ex.Message}"));
            SociCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Soci: {ex.Message}"));
            ApriGiornataCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Apertura Giornata: {ex.Message}"));
            //CassaCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Cassa: {ex.Message}"));


        }

        // ---------------------------------------------------------------------
        // 4. Ciclo di Vita (Override dei Metodi Virtuali della Base)
        // ---------------------------------------------------------------------
        
        protected override async Task OnLoading()
        {
            if (GlobalValuesC.MySetting == null) return;

            AttivaPermessi();

            // Caricamento dei dati asincroni passando correttamente il Token della base
            var listaDto = await Q.CaricaPostazioniCassa(GlobalValuesC.MySetting.IDOPERATORE, Token);

            CassaPostazioniDataSource = [.. listaDto.Select(dto => new MenuPostazioneMap(dto))];

            ApriGiornataEnabled = !(await Q.EsisteGiornataAperta(Token));

            if (GlobalValuesC.MySetting.POSTAZIONI?.Count == 0)
            {
                ApriPostazioneEnabled = false;
            }
        }
        protected override async Task OnEsc() => await GoTo(_menuToLogin);
        protected override void OnFinalDestruction()
        {
            CassaPostazioniDataSource?.Clear();
            Q = null;
            _host = null;

            base.OnFinalDestruction();
        }

        // 1. Aggiungi questo Subject per notificare l'esterno
        private readonly Subject<Unit> _menuToLogin = new();
        public IObservable<Unit> MenuToLogin => _menuToLogin.AsObservable();

        private readonly Subject<Unit> _menuToSoci = new();
        public IObservable<Unit> MenuToSoci => _menuToSoci.AsObservable();

        private readonly Subject<Unit> _menuToConnection = new();
        public IObservable<Unit> MenuToConnection => _menuToConnection.AsObservable();

        private readonly Subject<Unit> _menuToConfigurazione = new();
        public IObservable<Unit> MenuToConfigurazione => _menuToConfigurazione.AsObservable();

        private readonly Subject<Unit> _menuToCassa = new();
        public IObservable<Unit> MenuToCassa => _menuToCassa.AsObservable();

        private void AttivaPermessi()
        {
            if (GlobalValuesC.MySetting is null) return;

            OperatoreName = "Operatore : " + GlobalValuesC.MySetting.NOMEOPERATORE;
            //SessioneContabile = "Sessione Contabile " + (ApriGiornataEnabled ? "Chiusa" : "Aperta");

            if (GlobalValuesC.MySetting.POSTAZIONI is null) return;

            try
            {
                foreach (PostazioneXC Element in GlobalValuesC.MySetting.POSTAZIONI)
                {
                    switch (Element.TIPOPOSTAZIONE)
                    {
                        case (int)Enums.Postazioni.Amministratore:
                            AmministratoreVisible = true;
                            ReportVisible = true;
                            break;

                        case (int)Enums.Postazioni.Cassa:
                            CassaVisible = true;
                            ReportVisible = true;
                            break;

                        case (int)Enums.Postazioni.Bar:
                            BarVisible = true;
                            break;

                        case (int)Enums.Postazioni.Guardaroba:
                            GuardarobaVisible = true;
                            break;

                        case (int)Enums.Postazioni.Pulizie:
                            PulizieVisible = true;
                            break;

                    }
                }
            }
            catch (NullReferenceException)
            {
                return;
            }

            IsMenuReady = true;


        }

                

    }

    public partial class MenuViewModel
    {
        // ---------------------------------------------------------------------
        // 5. Logica Interna (Task dei Comandi)
        // ---------------------------------------------------------------------
        private async Task GoToCassa(int postazioneId)
        {
            _isClosing = true;
            _menuToCassa.OnNext(Unit.Default);
            _menuToCassa.OnCompleted();
            await Task.CompletedTask;

            //var cassaVm = Locator.Current.GetService<ICassaViewModel>();
            //if (cassaVm != null)
            //{
            //    cassaVm.SetHost(_host);
            //    cassaVm.SetPostazioneId(postazioneId);
            //    try
            //    {
            //        await _host.Router.NavigateAndReset.Execute(cassaVm);
            //    }
            //    catch (Exception ex)
            //    {
            //        _isClosing = false;
            //        Debug.WriteLine($"ERRORE durante la navigazione alla Cassa: {ex.Message}");
            //    }
            //}
            //else
            //{
            //    _isClosing = false; // Permette all'utente di riprovare se il DI fallisce
            //    Debug.WriteLine("ERRORE CRITICO: ICassaViewModel non è stato risolto dal Locator.");
            //}
            //await Task.CompletedTask;
            //await HostScreen.Router.NavigateAndReset.Execute(new CassaViewModel(HostScreen, postazioneId));

            await Task.CompletedTask;
        }

        private async Task GoTo(Subject<Unit> navigationSubject)
        {
            _isClosing = true; // Impedisce ulteriori interazioni durante la navigazione
            navigationSubject.OnNext(Unit.Default);
            navigationSubject.OnCompleted(); // Completa il flusso per notificare l'esterno
            await Task.CompletedTask;
        }

        

        private async Task ExecuteOpenGiornata()
        {
            // Utilizzo del Task.Run combinato con il Token ereditato per preservare la reattività della UI
            bool result = await Task.Run(() => Q.OpenGiornata(Token), Token);
            if (result)
            {
                ApriGiornataEnabled = false;
            }
        }
    }

    public partial class MenuViewModel
    {
        // ---------------------------------------------------------------------
        // 1. Visibilità dei Moduli e Permessi del Menu
        // ---------------------------------------------------------------------
        #region Visibility Properties

        private List<bool> _visibile = [];
        public List<bool> Visibile
        {
            get => _visibile;
            set => this.RaiseAndSetIfChanged(ref _visibile, value);
        }

        private bool _myamministratorevisible;
        public bool AmministratoreVisible
        {
            get => _myamministratorevisible;
            set => this.RaiseAndSetIfChanged(ref _myamministratorevisible, value);
        }

        private bool _myreportvisible;
        public bool ReportVisible
        {
            get => _myreportvisible;
            set => this.RaiseAndSetIfChanged(ref _myreportvisible, value);
        }

        private bool _mycassavisible;
        public bool CassaVisible
        {
            get => _mycassavisible;
            set => this.RaiseAndSetIfChanged(ref _mycassavisible, value);
        }

        private bool _mybarvisible;
        public bool BarVisible
        {
            get => _mybarvisible;
            set => this.RaiseAndSetIfChanged(ref _mybarvisible, value);
        }

        private bool _myguardarobavisible;
        public bool GuardarobaVisible
        {
            get => _myguardarobavisible;
            set => this.RaiseAndSetIfChanged(ref _myguardarobavisible, value);
        }

        private bool _mypulizievisible;
        public bool PulizieVisible
        {
            get => _mypulizievisible;
            set => this.RaiseAndSetIfChanged(ref _mypulizievisible, value);
        }

        #endregion

        // ---------------------------------------------------------------------
        // 2. Dati Operatore e Postazioni (Cassa)
        // ---------------------------------------------------------------------
        #region Operator and Workstation Data

        private string _myoperatorename = string.Empty;
        public string OperatoreName
        {
            get => _myoperatorename;
            set => this.RaiseAndSetIfChanged(ref _myoperatorename, value);
        }

        private List<MenuPostazioneMap> _mycassapostazionidatasource = null;
        public List<MenuPostazioneMap> CassaPostazioniDataSource
        {
            get => _mycassapostazionidatasource;
            set => this.RaiseAndSetIfChanged(ref _mycassapostazionidatasource, value);
        }

        private MenuPostazioneMap _selectedPostazione;
        public MenuPostazioneMap SelectedPostazione
        {
            get => _selectedPostazione;
            set => this.RaiseAndSetIfChanged(ref _selectedPostazione, value);
        }

        #endregion

        // ---------------------------------------------------------------------
        // 3. Gestione Stato Sessione Contabile (Giornata / Postazione)
        // ---------------------------------------------------------------------
        #region Accounting Session Properties

        // Definizioni degli OAPH (saranno valorizzati tramite .ToProperty() nel costruttore)
        private readonly ObservableAsPropertyHelper<string> _sessioneContabile;
        public string SessioneContabile => _sessioneContabile.Value;

        private readonly ObservableAsPropertyHelper<bool> _chiudiGiornataEnabled;
        public bool ChiudiGiornataEnabled => _chiudiGiornataEnabled.Value;

        private bool _apriGiornataEnabled;
        public bool ApriGiornataEnabled
        {
            get => _apriGiornataEnabled;
            set => this.RaiseAndSetIfChanged(ref _apriGiornataEnabled, value);
        }

        private bool _myapripostazioneenabled = false;
        public bool ApriPostazioneEnabled
        {
            get => _myapripostazioneenabled;
            set => this.RaiseAndSetIfChanged(ref _myapripostazioneenabled, value);
        }

        #endregion

        // ---------------------------------------------------------------------
        // 4. Stato Generale del Menu
        // ---------------------------------------------------------------------
        #region General UI State

        private bool _isMenuReady = false;
        public bool IsMenuReady
        {
            get => _isMenuReady;
            set => this.RaiseAndSetIfChanged(ref _isMenuReady, value);
        }

        #endregion
    }


}
