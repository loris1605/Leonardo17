using Contracts;
using Login.Core.Repository;
using Login.ViewModels.Map;
using ReactiveUI;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ViewModels;
using ViewModelServices; // necessario per RunOnMainThread

namespace Login.ViewModels
{

    public partial class LoginViewModel(ILoginRepository Repository) : ViewModelBase(), ILoginViewModel
    {
        // ---------------------------------------------------------------------
        // 1. Dipendenze e Campi Privati
        // ---------------------------------------------------------------------
        private ILoginRepository Q = Repository ?? throw new ArgumentNullException(nameof(Repository));


        // ---------------------------------------------------------------------
        // 3. Condizioni di Esecuzione (Override)
        // ---------------------------------------------------------------------
        protected override IObservable<bool> CanSave => this.WhenAnyValue(
            x => x.PasswordText,
            x => x.BindingT,
            (pass, operatore) =>
                !string.IsNullOrWhiteSpace(pass) &&
                operatore != null &&
                pass == operatore.Password)
            // Evita che ogni singolo carattere digitato intasi il flusso CombineLatest della base
            .DistinctUntilChanged()
            .ObserveOn(RxSchedulers.MainThreadScheduler);

        protected override void OnFinalDestruction()
        {
            // Pulizia esplicita per agevolare il Garbage Collector forzato della Base
            try
            {
                _loginSuccesso?.OnCompleted();
                _loginSuccesso?.Dispose();
            }
            catch { /* silenzioso */ }

            Q = null;
            DataSource = null;
            BindingT = null;
            PasswordText = string.Empty;

            base.OnFinalDestruction();
        }

        // ---------------------------------------------------------------------
        // 4. Ciclo di Vita (Override dei Metodi Virtuali)
        // ---------------------------------------------------------------------
        protected override async Task OnLoading()
        {
            var dbData = await Q.GetOperatoriAbilitati(Token).ConfigureAwait(false);

            if (Token.IsCancellationRequested) return;

            if (dbData?.Count > 0)
            {
                // Mappatura CPU-bound eseguita in background
                var localList = dbData.Select(dto => new LoginMap(dto)).ToList();

                // Aggiorna le proprietà della UI sul Main Thread per evitare cross-thread issues
                await RxSchedulers.MainThreadScheduler.RunOnMainThread<Unit>(() =>
                {
                    DataSource = localList;
                    // Piccolo delay UI non necessario quando si esegue sul MainThread, ma manteniamo la logica
                    BindingT = localList.Count > 0 ? localList[0] : null;
                    return Unit.Default;
                });

                if (Token.IsCancellationRequested) return;
            }

            if (!_isClosing && !Token.IsCancellationRequested)
            {
                await SetFocus(PasswordFocus);
            }

        }

        protected override async Task OnSaving()
        {

            try
            {
                if (BindingT is null)
                {
                    Debug.WriteLine("OnSaving chiamato con BindingT nullo, nulla da salvare.");
                    return;
                }

                // Salva le impostazioni dell'operatore selezionato (esegue IO; non blocca UI)
                await Q.SaveSettings(BindingT.ToDto(), Token).ConfigureAwait(false);

                // Naviga al Menu principale resettando lo stack di navigazione
                // 2. Al posto di GoToMenu(), suoniamo il campanello!
                _isClosing = true;
                _loginSuccesso.OnNext(Unit.Default);
                _loginSuccesso.OnCompleted(); // Chiude il canale per sempre
            }
            catch (OperationCanceledException)
            {
                _isClosing = false;
                Debug.WriteLine("Salvataggio login annullato tramite Token.");
            }
            catch (Exception ex)
            {
                _isClosing = false;
                Debug.WriteLine($">>> [ERROR] Login fallito durante il salvataggio o la navigazione: {ex.Message}");
                // Qui potresti aggiungere un'interaction per mostrare un messaggio di errore all'utente
                throw; // Rilancia l'eccezione se vuoi che venga gestita a un livello superiore
            }

        }



        protected override Task OnEsc()
        {
            OnAppShutDown(); // Riutilizza il metodo centralizzato della base per spegnere l'app
            return Task.CompletedTask;
        }

    }

    public partial class LoginViewModel
    {
        // 1. Aggiungi questo Subject per notificare l'esterno
        private readonly Subject<Unit> _loginSuccesso = new();
        public IObservable<Unit> LoginSuccesso => _loginSuccesso.AsObservable();

        // ---------------------------------------------------------------------
        // 2. Proprietà e Stato della UI (con Bindings)
        // ---------------------------------------------------------------------
        private string _passwordText = string.Empty;
        public string PasswordText
        {
            get => _passwordText;
            set => this.RaiseAndSetIfChanged(ref _passwordText, value);
        }

        // Nota: LoginMap ha costruttore parameterless, inizializziamo per evitare null-reference nelle binding
        private LoginMap _bindingT = null!;
        public LoginMap BindingT
        {
            get => _bindingT;
            set => this.RaiseAndSetIfChanged(ref _bindingT, value);
        }

        private List<LoginMap> _dataSource = [];
        public List<LoginMap> DataSource
        {
            get => _dataSource;
            set => this.RaiseAndSetIfChanged(ref _dataSource, value);
        }

        // Interazioni con la View
        public Interaction<Unit, Unit> PasswordFocus { get; } = new();

    }
}
