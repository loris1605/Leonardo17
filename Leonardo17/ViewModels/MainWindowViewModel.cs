using Contracts;
using Core.Repository;
using Leonardo17;
using Microsoft.EntityFrameworkCore;
using Models.Context;
using Models.Repository;
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
using System.Threading.Tasks;

namespace ViewModels
{
    public interface IMainWindowViewModel : IRoutableViewModel { }

    public partial class MainWindowViewModel : ViewModelBase,
                                                   IScreen,
                                                   IRoutableViewModel,
                                                   IActivatableViewModel,
                                                   IMainWindowViewModel
    {
        private readonly ISettingRepository _settingRepository;
        private bool _isInitialized;

        private CompositeDisposable _currentNavigationDisposables = new();

        // Iniettiamo le interfacce dei ViewModel per la navigazione

        public RoutingState Router { get; } = new RoutingState();

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
                    // Task.Run(async () => await InitializeNavigation()); lasciamo che sia OnLoading a gestire
                }

                Disposable.Create(() =>
                {
                    ResetNavigationDisposables();
                }).DisposeWith(disposables);
            });
        }

        private void ResetNavigationDisposables()
        {
            try
            {
                _currentNavigationDisposables.Dispose();
            }
            catch
            {
                // eventualmente loggare, ma non rilanciare
            }

            _currentNavigationDisposables = new();
        }

        protected override async Task OnLoading()
        {
            await InitializeNavigation().ConfigureAwait(false);
        }

        private async Task InitializeNavigation()
        {
            Debug.WriteLine(">>> [MainVM] InitializeNavigation START");

            try
            {
                await Task.Run(() => AppServices.Connection.TestConnection());

                if (AppServices.Flags.ServerAttivo)
                {
                    Debug.WriteLine(">>> [MainVM] Database esistente, controllo migrazioni...");
                    if (await DatabaseHelper.HasPendingMigrationsAsync(Token))
                    {
                        await DatabaseHelper.ApplyMigrationsIfNeededAsync(Token);
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

            Debug.WriteLine(">>> [MainVM] InitializeNavigation END");
        }

        private static async Task VerificaNecessitaAggiornamento()
        {
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

            await Task.Run(() => ModuleLoader.EnsureConnectionModuleLoaded());

            var tcs = new TaskCompletionSource<Unit>(TaskCreationOptions.RunContinuationsAsynchronously);

            RxSchedulers.MainThreadScheduler.Schedule(() =>
            {
                try
                {
                    var connectionVM = Locator.Current.GetService<IConnectionViewModel>();

                    if (connectionVM == null)
                    {
                        Debug.WriteLine(">>> [ERROR] Impossibile risolvere IConnectionViewModel.");
                        tcs.TrySetResult(Unit.Default);
                        return;
                    }

                    connectionVM.ConnectionToLogin
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .SelectMany(_ => Observable.FromAsync(GoToLogin))
                        .Subscribe()
                        .DisposeWith(_currentNavigationDisposables);

                    Router.NavigateAndReset.Execute(connectionVM)
                        .Select(_ => Unit.Default)
                        .Subscribe(
                            _ => { },
                            ex => tcs.TrySetException(ex),
                            () => tcs.TrySetResult(Unit.Default)
                        );
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            await tcs.Task;
        }

        private async Task GoToLogin()
        {
            _currentNavigationDisposables.Clear();

            await Task.Run(() => ModuleLoader.EnsureLoginModuleLoaded());

            var tcs = new TaskCompletionSource<Unit>(TaskCreationOptions.RunContinuationsAsynchronously);

            RxSchedulers.MainThreadScheduler.Schedule(() =>
            {
                try
                {
                    var loginVM = Locator.Current.GetService<ILoginViewModel>();

                    if (loginVM == null)
                    {
                        Debug.WriteLine(">>> [ERROR] Impossibile risolvere ILoginViewModel.");
                        tcs.TrySetResult(Unit.Default);
                        return;
                    }

                    loginVM.LoginSuccesso
                        .Take(1)
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .SelectMany(_ => Observable.FromAsync(GoToMenu))
                        .Subscribe()
                        .DisposeWith(_currentNavigationDisposables);

                    Router.NavigateAndReset.Execute(loginVM)
                        .Select(_ => Unit.Default)
                        .Subscribe(
                            _ => { },
                            ex => tcs.TrySetException(ex),
                            () => tcs.TrySetResult(Unit.Default)
                        );
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            await tcs.Task;
        }

        private async Task GoToMenu()
        {
            _currentNavigationDisposables.Clear();

            await Task.Run(() => ModuleLoader.EnsureMenuModuleLoaded());

            var tcs = new TaskCompletionSource<Unit>(TaskCreationOptions.RunContinuationsAsynchronously);

            RxSchedulers.MainThreadScheduler.Schedule(() =>
            {
                try
                {
                    var menuVM = Locator.Current.GetService<IMenuViewModel>();

                    if (menuVM == null)
                    {
                        Debug.WriteLine(">>> [ERROR] Impossibile risolvere IMenuViewModel.");
                        tcs.TrySetResult(Unit.Default);
                        return;
                    }

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

                    menuVM.MenuToCassa
                        .Take(1)
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .SelectMany(postazioneId => Observable.FromAsync(() => GoToCassa(postazioneId)))
                        .Subscribe()
                        .DisposeWith(_currentNavigationDisposables);

                    Router.NavigateAndReset.Execute(menuVM)
                        .Select(_ => Unit.Default)
                        .Subscribe(
                            _ => { },
                            ex => tcs.TrySetException(ex),
                            () => tcs.TrySetResult(Unit.Default)
                        );
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            await tcs.Task;
        }

        private async Task GoToSoci()
        {
            _currentNavigationDisposables.Clear();

            await Task.Run(() => ModuleLoader.EnsureSociModuleLoaded());

            var tcs = new TaskCompletionSource<Unit>(TaskCreationOptions.RunContinuationsAsynchronously);

            RxSchedulers.MainThreadScheduler.Schedule(() =>
            {
                try
                {
                    var sociVM = Locator.Current.GetService<ISociViewModel>();

                    if (sociVM == null)
                    {
                        Debug.WriteLine(">>> [ERROR] Impossibile risolvere ISociViewModel.");
                        tcs.TrySetResult(Unit.Default);
                        return;
                    }

                    sociVM.SociToMenu
                        .Take(1)
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .SelectMany(_ => Observable.FromAsync(GoToMenu))
                        .Subscribe()
                        .DisposeWith(_currentNavigationDisposables);

                    Router.NavigateAndReset.Execute(sociVM)
                        .Select(_ => Unit.Default)
                        .Subscribe(
                            _ => { },
                            ex => tcs.TrySetException(ex),
                            () => tcs.TrySetResult(Unit.Default)
                        );
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            await tcs.Task;
        }

        private async Task GoToConfigurazione()
        {
            _currentNavigationDisposables.Clear();

            await Task.Run(() => ModuleLoader.EnsureConfigurazioneModuleLoaded());

            var tcs = new TaskCompletionSource<Unit>(TaskCreationOptions.RunContinuationsAsynchronously);

            RxSchedulers.MainThreadScheduler.Schedule(() =>
            {
                try
                {
                    var configurazioneVM = Locator.Current.GetService<IConfigurazioneViewModel>();

                    if (configurazioneVM != null)
                    {
                        configurazioneVM.ConfigurazioneToMenu
                            .Take(1)
                            .ObserveOn(RxSchedulers.MainThreadScheduler)
                            .SelectMany(_ => Observable.FromAsync(GoToMenu))
                            .Subscribe()
                            .DisposeWith(_currentNavigationDisposables);

                        Router.NavigateAndReset.Execute(configurazioneVM)
                            .Select(_ => Unit.Default)
                            .Subscribe(
                                _ => { },
                                ex => tcs.TrySetException(ex),
                                () => tcs.TrySetResult(Unit.Default)
                            );
                    }
                    else
                    {
                        Debug.WriteLine(">>> [ERROR] Impossibile risolvere IConfigurazioneViewModel.");
                        tcs.TrySetResult(Unit.Default);
                    }
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            await tcs.Task;
        }

        private async Task GoToCassa(int postazioneId)
        {
            _currentNavigationDisposables.Clear();

            await Task.Run(() => ModuleLoader.EnsureCassaModuleLoaded());

            var tcs = new TaskCompletionSource<Unit>(TaskCreationOptions.RunContinuationsAsynchronously);

            RxSchedulers.MainThreadScheduler.Schedule(() =>
            {
                try
                {
                    var cassaVM = Locator.Current.GetService<ICassaViewModel>();

                    if (cassaVM != null)
                    {
                        cassaVM.SetPostazioneId(postazioneId);

                        cassaVM.CassaToMenu
                            .Take(1)
                            .ObserveOn(RxSchedulers.MainThreadScheduler)
                            .SelectMany(_ => Observable.FromAsync(GoToMenu))
                            .Subscribe()
                            .DisposeWith(_currentNavigationDisposables);

                        Router.NavigateAndReset.Execute(cassaVM)
                            .Select(_ => Unit.Default)
                            .Subscribe(
                                _ => { },
                                ex => tcs.TrySetException(ex),
                                () => tcs.TrySetResult(Unit.Default)
                            );
                    }
                    else
                    {
                        Debug.WriteLine(">>> [ERROR] Impossibile risolvere ICassaViewModel.");
                        tcs.TrySetResult(Unit.Default);
                    }
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            await tcs.Task;
        }

    }

}
