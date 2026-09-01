using Contracts;
using Login.Core.Repository;
using Login.ViewModels.Map;
using ReactiveUI;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ViewModels;
using ViewModelServices; // RunOnMainThread

namespace Login.ViewModels
{

    public partial class LoginViewModel(ILoginRepository Repository) : ViewModelBase(), ILoginViewModel
    {
        // ---------------------------------------------------------------------
        // 1. Dipendenze e Campi Privati
        // ---------------------------------------------------------------------
        private readonly ILoginRepository Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
        private readonly CompositeDisposable _disposables = new();

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
            .DistinctUntilChanged()
            .ObserveOn(RxSchedulers.MainThreadScheduler);

        protected override void OnFinalDestruction()
        {
            // Pulizia esplicita per agevolare il Garbage Collector della Base
            try
            {
                try
                {
                    // Completiamo il flusso prima di rilasciare le risorse
                    _loginSuccesso.OnCompleted();
                }
                catch (ObjectDisposedException) { /* già rilasciato, silenzioso */ }
                catch (Exception ex)
                {
                    Debug.WriteLine($">>> [WARN] Errore OnCompleted _loginSuccesso: {ex.Message}");
                }

                try
                {
                    _loginSuccesso.Dispose();
                }
                catch (ObjectDisposedException) { /* già rilasciato, silenzioso */ }
                catch (Exception ex)
                {
                    Debug.WriteLine($">>> [WARN] Errore Dispose _loginSuccesso: {ex.Message}");
                }

                // Dispose delle sottoscrizioni locali (CompositeDisposable è non-nullable)
                try
                {
                    _disposables.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($">>> [WARN] Errore Dispose _disposables: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> [WARN] Errore durante OnFinalDestruction: {ex.Message}");
            }

            // Non impostare a null campi non-nullable: svuota o resetta in modo safe
            try
            {
                DataSource?.Clear();
            }
            catch { /* silenzioso */ }

            BindingT = null;
            PasswordText = string.Empty;

            // Non impostare Q = null (è non-nullable). Se necessita cleanup, gestiscilo nel DI o in IDisposable dell'implementazione.
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
                // Mappatura CPU-bound in background
                var localList = dbData.Select(dto => new LoginMap(dto)).ToList();

                // Assegna le proprietà UI sul Main Thread per evitare cross-thread issues
                await RxSchedulers.MainThreadScheduler.RunOnMainThread(() =>
                {
                    DataSource = localList;
                    BindingT = localList.Count > 0 ? localList[0] : null;
                    return Unit.Default;
                }).ConfigureAwait(false);

                if (Token.IsCancellationRequested) return;
            }
            else
            {
                await RxSchedulers.MainThreadScheduler.RunOnMainThread(() =>
                {
                    DataSource = new List<LoginMap>();
                    BindingT = null;
                    return Unit.Default;
                }).ConfigureAwait(false);
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

                // Salva le impostazioni dell'operatore selezionato (IO-bound)
                await Q.SaveSettings(BindingT.ToDto(), Token).ConfigureAwait(false);

                // Notifica di login riuscito (sul Main Thread)
                await RxSchedulers.MainThreadScheduler.RunOnMainThread(() =>
                {
                    _isClosing = true;
                    _loginSuccesso.OnNext(Unit.Default);
                    _loginSuccesso.OnCompleted();
                    return Unit.Default;
                }).ConfigureAwait(false);
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
                throw;
            }
        }

        protected override Task OnEsc()
        {
            OnAppShutDown();
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

        private LoginMap? _bindingT;
        public LoginMap? BindingT
        {
            get => _bindingT;
            set => this.RaiseAndSetIfChanged(ref _bindingT, value);
        }

        private List<LoginMap> _dataSource = new();
        public List<LoginMap> DataSource
        {
            get => _dataSource;
            set => this.RaiseAndSetIfChanged(ref _dataSource, value);
        }

        // Interazioni con la View
        public Interaction<Unit, Unit> PasswordFocus { get; } = new();

    }
}
