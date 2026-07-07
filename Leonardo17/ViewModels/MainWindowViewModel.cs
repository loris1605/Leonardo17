using Contracts;
using Core.Repository;
using Leonardo17;
using Microsoft.EntityFrameworkCore;
using Models.Context;
using ReactiveUI;
using Splat;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace ViewModels
{
    public interface IMainWindowViewModel : IRoutableViewModel { }

    public partial class MainWindowViewModel : ReactiveObject, 
                                                   IScreen,
                                                   IRoutableViewModel,
                                                   IActivatableViewModel,
                                                   IMainWindowViewModel          

    {

        readonly int _currentVersion = 1; // Versione attuale del database, da aggiornare quando si modificano le entità
        private readonly ISettingRepository _settingRepository;
        private bool _isInitialized;

        private readonly CompositeDisposable _currentNavigationDisposables = [];

        // Iniettiamo le interfacce dei ViewModel per la navigazione

        public RoutingState Router { get; } = new RoutingState();
        public string UrlPathSegment => "main";
        public IScreen HostScreen => this;
        public ViewModelActivator Activator { get; } = new();

        public MainWindowViewModel(ISettingRepository settingRepository)
        {
            _settingRepository = settingRepository ?? throw new ArgumentNullException(nameof(settingRepository));
            
            // Spostiamo la logica di navigazione all'attivazione
            this.WhenActivated(disposables =>
            {
                if (!_isInitialized)
                {
                    _isInitialized = true;
                    // Avviamo l'inizializzazione sul thread di background per la parte DB/Rete
                    // e ci spostiamo sulla UI solo quando andiamo a navigare.
                    Task.Run(async () => await InitializeNavigation());
                }

                Disposable.Create(() =>
                {
                  _currentNavigationDisposables.Dispose();
                }).DisposeWith(disposables);

            });
        }

        private async Task InitializeNavigation()
        {
            try
            {
                await Task.Run(() => AppServices.Connection.TestConnection());

                if (AppServices.Flags.ServerAttivo)
                {
                    // 2. Controllo versione ed eventuali migrazioni
                    if (!await _settingRepository.CheckAppVersion(_currentVersion))
                    {
                        await VerificaNecessitaAggiornamento();
                    }

                    await GoToLogin();
                }
                else
                {
                    await GoToConnection();

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Errore critico durante InitializeNavigation: {ex.Message}");
                // Qui potresti navigare verso una ErrorView generica se necessario
            }
        }

        

        // Modificato in statico o assicurati che l'istanza di AppDbContext sia configurata correttamente
        private static async Task VerificaNecessitaAggiornamento()
        {
            // Nota: AppDbContext andrebbe idealmente iniettato tramite una Factory (IDbContextFactory)
            // per evitare problemi di scope, specialmente in app desktop.
            using var ctx = new AppDbContext();
            if (await ctx.Database.CanConnectAsync())
            {
                var pending = await ctx.Database.GetPendingMigrationsAsync();
                if (pending.Any())
                {
                    await ctx.Database.MigrateAsync();
                }
            }
        }
     

    }

    public partial class MainWindowViewModel
    {
        private async Task GoToConnection()
        {
            _currentNavigationDisposables.Clear();

            // 1. Carica la DLL in background
            await Task.Run(() => ModuleLoader.EnsureConnectionModuleLoaded());

            // Utilizziamo un TCS per coordinare l'uscita asincrona dal metodo
            var tcs = new TaskCompletionSource<Unit>();

            // 2. Usiamo .Schedule() SINCRONO (Senza 'await' davanti)
            RxSchedulers.MainThreadScheduler.Schedule(() =>
            {
                try
                {
                    // Il costruttore del ConnectionViewModel viene eseguito sul thread UI
                    var connectionVM = Locator.Current.GetService<IConnectionViewModel>();

                    if (connectionVM == null)
                    {
                        Debug.WriteLine(">>> [ERROR] Impossibile risolvere IConnectionViewModel.");
                        tcs.SetResult(Unit.Default);
                        return;
                    }

                    // 3. Sottoscrizione pulita e sicura all'evento prima della navigazione
                    connectionVM.ConnectionToLogin
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .SelectMany(_ => Observable.FromAsync(GoToLogin))
                        .Subscribe()
                        .DisposeWith(_currentNavigationDisposables);

                    // 4. Esecuzione della navigazione e gestione del completamento del Task
                    Router.NavigateAndReset.Execute(connectionVM)
                        .Select(_ => Unit.Default)
                        .Subscribe(
                            _ => { },
                            ex => tcs.SetException(ex),
                            () => tcs.SetResult(Unit.Default) // Notifica che la navigazione è completata
                        );
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            // 5. Il metodo attende qui finché la navigazione sul thread UI non è conclusa
            await tcs.Task;
        }

        private async Task GoToLogin()
        {
            _currentNavigationDisposables.Clear();

            // 1. Carica la DLL in background
            await Task.Run(() => ModuleLoader.EnsureLoginModuleLoaded());

            // Utilizziamo un TCS per coordinare l'uscita asincrona dal metodo
            var tcs = new TaskCompletionSource<Unit>();

            // 2. Usiamo .Schedule() SINCRONO (Senza 'await' davanti)
            RxSchedulers.MainThreadScheduler.Schedule(() =>
            {
                try
                {
                    // Il costruttore del LoginViewModel viene eseguito sul thread UI
                    var loginVM = Locator.Current.GetService<ILoginViewModel>();

                    if (loginVM == null)
                    {
                        Debug.WriteLine(">>> [ERROR] Impossibile risolvere ILoginViewModel.");
                        tcs.SetResult(Unit.Default);
                        return;
                    }

                    // 3. Sottoscrizione pulita e sicura all'evento di login successo PRIMA della navigazione
                    loginVM.LoginSuccesso
                        .Take(1)
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .SelectMany(_ => Observable.FromAsync(GoToMenu))
                        .Subscribe()
                        .DisposeWith(_currentNavigationDisposables);

                    // 4. Esecuzione della navigazione e gestione del completamento del Task
                    Router.NavigateAndReset.Execute(loginVM)
                        .Select(_ => Unit.Default)
                        .Subscribe(
                            _ => { },
                            ex => tcs.SetException(ex),
                            () => tcs.SetResult(Unit.Default) // Notifica che la navigazione è completata
                        );
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            // 5. Il metodo attende qui finché la navigazione sul thread UI non è conclusa
            await tcs.Task;
        }

        private async Task GoToMenu()
        {
            _currentNavigationDisposables.Clear();

            // 1. Carica la DLL in background
            await Task.Run(() => ModuleLoader.EnsureMenuModuleLoaded());

            // Utilizziamo un TCS per coordinare l'uscita asincrona dal metodo
            var tcs = new TaskCompletionSource<Unit>();

            // 2. Usiamo .Schedule() SINCRONO (Rimosso 'await' da qui davanti)
            RxSchedulers.MainThreadScheduler.Schedule(() =>
            {
                try
                {
                    var menuVM = Locator.Current.GetService<IMenuViewModel>();

                    if (menuVM == null)
                    {
                        Debug.WriteLine(">>> [ERROR] Impossibile risolvere IMenuViewModel.");
                        tcs.SetResult(Unit.Default);
                        return;
                    }

                    // 3. Sottoscrizioni pulite e sicure
                    menuVM.MenuToLogin
                        .Take(1)
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .SelectMany(_ => Observable.FromAsync(GoToLogin))
                        .Subscribe()
                        .DisposeWith(_currentNavigationDisposables);

                    menuVM.MenuToSoci
                        .Take(1)
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .SelectMany(_ => Observable.FromAsync(GoToSoci))
                        .Subscribe()
                        .DisposeWith(_currentNavigationDisposables);

                    menuVM.MenuToConnection
                        .Take(1)
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .SelectMany(_ => Observable.FromAsync(GoToConnection))
                        .Subscribe()
                        .DisposeWith(_currentNavigationDisposables);

                    menuVM.MenuToConfigurazione
                        .Take(1)
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .SelectMany(_ => Observable.FromAsync(GoToConfigurazione))
                        .Subscribe()
                        .DisposeWith(_currentNavigationDisposables);

                    // 4. Esecuzione della navigazione e gestione del completamento del Task
                    Router.NavigateAndReset.Execute(menuVM)
                        .Select(_ => Unit.Default)
                        .Subscribe(
                            _ => { },
                            ex => tcs.SetException(ex),
                            () => tcs.SetResult(Unit.Default) // Notifica che la navigazione è completata
                        );
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            // 5. Il metodo attende qui finché la navigazione sul thread UI non è conclusa
            await tcs.Task;
        }


        private async Task GoToSoci()
        {
            _currentNavigationDisposables.Clear();

            // 1. Carica la DLL in background
            await Task.Run(() => ModuleLoader.EnsureSociModuleLoaded());

            // Utilizziamo un TCS tipizzato per coordinare l'uscita asincrona dal metodo
            var tcs = new TaskCompletionSource<Unit>();

            // 2. Usiamo .Schedule() SINCRONO (Senza 'await' davanti)
            RxSchedulers.MainThreadScheduler.Schedule(() =>
            {
                try
                {
                    // Il costruttore del SociViewModel viene eseguito sul thread UI
                    var sociVM = Locator.Current.GetService<ISociViewModel>();

                    if (sociVM == null)
                    {
                        Debug.WriteLine(">>> [ERROR] Impossibile risolvere ISociViewModel.");
                        tcs.SetResult(Unit.Default);
                        return;
                    }

                    // 3. Sottoscrizione pulita e reattiva all'evento di ritorno al Menu PRIMA della navigazione
                    sociVM.SociToMenu
                        .Take(1)
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .SelectMany(_ => Observable.FromAsync(GoToMenu))
                        .Subscribe()
                        .DisposeWith(_currentNavigationDisposables);

                    // 4. Esecuzione della navigazione e gestione del completamento del Task
                    Router.NavigateAndReset.Execute(sociVM)
                        .Select(_ => Unit.Default)
                        .Subscribe(
                            _ => { },
                            ex => tcs.SetException(ex),
                            () => tcs.SetResult(Unit.Default) // Notifica che la navigazione è completata
                        );
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            // 5. Il metodo attende qui finché la navigazione sul thread UI non è conclusa
            await tcs.Task;
        }


        private async Task GoToConfigurazione()
        {
            _currentNavigationDisposables.Clear();

            // 1. Carica la DLL in background
            await Task.Run(() => ModuleLoader.EnsureConfigurazioneModuleLoaded());

            // Utilizziamo un TCS per coordinare l'uscita asincrona dal metodo
            var tcs = new TaskCompletionSource<Unit>();

            // 2. Usiamo .Schedule() SINCRONO (Senza 'await' davanti)
            RxSchedulers.MainThreadScheduler.Schedule(() =>
            {
                try
                {
                    // Il costruttore del ConfigurazioneViewModel viene eseguito sul thread UI
                    var configurazioneVM = Locator.Current.GetService<IConfigurazioneViewModel>();

                    if (configurazioneVM != null)
                    {
                        // 3. Sottoscrizione pulita e sicura all'evento di ritorno al Menu PRIMA della navigazione
                        configurazioneVM.ConfigurazioneToMenu
                            .Take(1)
                            .ObserveOn(RxSchedulers.MainThreadScheduler)
                            .SelectMany(_ => Observable.FromAsync(GoToMenu))
                            .Subscribe()
                            .DisposeWith(_currentNavigationDisposables);

                        // 4. Esecuzione della navigazione e gestione del completamento del Task
                        Router.NavigateAndReset.Execute(configurazioneVM)
                            .Select(_ => Unit.Default)
                            .Subscribe(
                                _ => { },
                                ex => tcs.SetException(ex),
                                () => tcs.SetResult(Unit.Default) // Notifica che la navigazione è completata
                            );
                    }
                    else
                    {
                        Debug.WriteLine(">>> [ERROR] Impossibile risolvere IConfigurazioneViewModel.");
                        tcs.SetResult(Unit.Default);
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            // 5. Il metodo attende qui finché la navigazione sul thread UI non è conclusa
            await tcs.Task;
        }

    }
}
