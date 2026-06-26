using Contracts;
using ReactiveUI;
using Splat;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ViewModels;

namespace Configurazione.ViewModels
{
    public interface IConfigurazioneScreen : IScreen
    {
        RoutingState GroupRouter { get; }
        RoutingState InputRouter { get; }
        bool GroupEnabled { get; set; }

        void AggiornaGridByInt(int id);
        
    }

    public interface IConfigurazioneCrudViewModel : IRoutableViewModel
    {
        void SetIdDaModificare(int id);
        void SetIdRitorno(int id);
        IObservable<Unit> InputEsc { get; }
        IObservable<int> InputBack { get; }
    }
    public partial class ConfigurazioneViewModel(IScreen host) : ViewModelBase(host), IConfigurazioneScreen, IConfigurazioneViewModel
    {
        // ---------------------------------------------------------------------
        // 1. Router Interni (Sub-Routing) e Dipendenze
        // ---------------------------------------------------------------------
        public RoutingState GroupRouter { get; } = new RoutingState();
        public RoutingState InputRouter { get; } = new RoutingState();

        // Espone il router principale richiesto dall'infrastruttura ReactiveUI
        public RoutingState Router => GroupRouter;

        private IScreen _host = host;
        public new IScreen HostScreen => _host;

        // ---------------------------------------------------------------------
        // 2. Controllo Esecuzione Centralizzato (Prevenzione Doppi Clic)
        // ---------------------------------------------------------------------
        protected override IObservable<bool> IsAnythingExecuting =>
            Observable.CombineLatest(
                // 1. Comandi base ereditati
                this.WhenAnyObservable(x => x.LoadCommand.IsExecuting).StartWith(false),
                this.WhenAnyObservable(x => x.SaveCommand.IsExecuting).StartWith(false),
                this.WhenAnyObservable(x => x.EscPressedCommand.IsExecuting).StartWith(false),
                // 2. Monitoraggio delle esecuzioni dei router (Navigazioni in corso)
                this.WhenAnyObservable(x => x.GroupRouter.NavigateAndReset.IsExecuting).StartWith(false),
                this.WhenAnyObservable(x => x.GroupRouter.Navigate.IsExecuting).StartWith(false),
                this.WhenAnyObservable(x => x.InputRouter.NavigateAndReset.IsExecuting).StartWith(false),
                // Se qualunque operazione o cambio pagina è attivo, blocca la UI
                (l, s, e, gReset, gNav, iReset) => l || s || e || gReset || gNav || iReset)
            .DistinctUntilChanged();

        // 1. Aggiungi questo Subject per notificare l'esterno
        private readonly Subject<Unit> _configurazioneToMenu = new();
        public IObservable<Unit> ConfigurazioneToMenu => _configurazioneToMenu.AsObservable();

        private readonly CompositeDisposable _navigationDisposables = [];

        // ---------------------------------------------------------------------
        // 3. Ciclo di Vita (Override dei Metodi Virtuali della Base)
        // ---------------------------------------------------------------------

        protected override void OnFinalDestruction()
        {
            // Svuotiamo gli stack di navigazione dei router interni per liberare le View collegate
            _navigationDisposables.Dispose();
            GroupRouter?.NavigationStack.Clear();
            InputRouter?.NavigationStack.Clear();
            _host = null;

            base.OnFinalDestruction();
        }

        protected override async Task OnLoading() => await GoToOperatoreGroup();
        protected override async Task OnSaving() => await Task.CompletedTask;
        protected override async Task OnEsc()
        {
            _isClosing = true;
            _configurazioneToMenu.OnNext(Unit.Default);
            _configurazioneToMenu.OnCompleted(); // Chiude il canale per sempre, prevenendo ulteriori notifiche

            await Task.CompletedTask;

        }

        // ---------------------------------------------------------------------
        // 4. Metodi di Interfaccia e Sincronizzazione Griglie (ISociScreen)
        // ---------------------------------------------------------------------
        public void AggiornaGridByObject(object model)
        {
            if (GroupRouter.GetCurrentViewModel() is IGroupViewModelBase groupVm)
            {
                groupVm.CaricaByModel(model);
            }
        }

        public void AggiornaGridByInt(int id)
        {
            if (GroupRouter.GetCurrentViewModel() is IGroupViewModelBase groupVm)
            {
                // Passiamo l'ID al metodo di caricamento della lista
                groupVm.CaricaDataSource(id);
            }
        }

    }

    public partial class ConfigurazioneViewModel
    {
        #region GroupEnabled

        private bool _groupenabled = true;
        public bool GroupEnabled
        {
            get => _groupenabled;
            set => this.RaiseAndSetIfChanged(ref _groupenabled, value);
        }

        #endregion

    }

    public partial class ConfigurazioneViewModel
    {
        private async Task GoToOperatoreGroup()
        {
            // 1. Puliamo subito i vecchi disposable (previene i memory leak delle vecchie view)
            _navigationDisposables.Clear();

            // 2. SOLUZIONE DOPPIO CLICK: Spegniamo subito l'interattività della view corrente
            GroupEnabled = false;
            await Task.Delay(200); // Assorbe il click "fantasma" hardware
            GroupEnabled = true;   // Riabilitiamo per la nuova schermata in arrivo

            try
            {
                // Risolviamo il ViewModel (può stare fuori dal thread UI, alleggerendolo)
                var groupVM = Locator.Current.GetService<IOperatoreGroupViewModel>();

                if (groupVM != null)
                {
                    // Colleghiamo l'evento del nuovo ViewModel al ciclo di vita controllato della classe
                    groupVM.OperatoreToPostazioni
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .Subscribe(async _ =>
                        {
                            GroupEnabled = false;
                            await GoToPostazioneGroup();
                        })
                        .DisposeWith(_navigationDisposables);

                    groupVM.OperatoreToSettori
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .Subscribe(async _ =>
                        {
                            GroupEnabled = false;
                            await GoToSettoreGroup();
                        })
                        .DisposeWith(_navigationDisposables);

                    groupVM.OperatoreToTariffe
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .Subscribe(async _ =>
                        {
                            GroupEnabled = false;
                            await GoToTariffaGroup();
                        })
                        .DisposeWith(_navigationDisposables);

                    groupVM.GroupToOperatoreAdd
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .Subscribe(async _ =>
                        {
                            GroupEnabled = false;
                            await GoToInput(Locator.Current.GetService<IOperatoreAddViewModel>());
                        })
                        .DisposeWith(_navigationDisposables);

                    groupVM.GroupToOperatoreDel
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .Subscribe(async id =>
                        {
                            GroupEnabled = false;
                            await GoToInput(Locator.Current.GetService<IOperatoreDelViewModel>(), id);
                        })
                        .DisposeWith(_navigationDisposables);

                    groupVM.GroupToOperatoreUpd
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .Subscribe(async id =>
                        {
                            GroupEnabled = false;
                            await GoToInput(Locator.Current.GetService<IOperatoreUpdViewModel>(), id);
                        })
                        .DisposeWith(_navigationDisposables);


                    // 3. NAVIGAZIONE SUL MAIN THREAD (Senza usare ScheduleAsync)
                    var tcs = new TaskCompletionSource();

                    RxSchedulers.MainThreadScheduler.Schedule(() =>
                    {
                        // Avviamo il comando sincrono del router di ReactiveUI 
                        // e usiamo la Subscribe per intercettare il completamento
                        Router.NavigateAndReset.Execute(groupVM)
                            .Subscribe(
                                _ => tcs.SetResult(), // Navigazione completata con successo
                                ex => tcs.SetException(ex) // Errore catturato
                            );
                    });

                    // Attendiamo in modo asincrono puro che la UI abbia finito lo switch visivo
                    await tcs.Task;
                }
                else
                {
                    Debug.WriteLine(">>> [ERROR] Impossibile risolvere IOperatoreGroupViewModel.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> [EXCEPTION] Errore durante la navigazione: {ex.Message}");
                throw;
            }
        }

        private async Task GoToPostazioneGroup()
        {
            // 1. Puliamo subito i vecchi disposable (previene i memory leak delle vecchie view)
            _navigationDisposables.Clear();

            // 2. SOLUZIONE DOPPIO CLICK: Spegniamo subito l'interattività della view corrente
            GroupEnabled = false;
            await Task.Delay(200); // Assorbe il click "fantasma" hardware
            GroupEnabled = true;   // Riabilitiamo per la nuova schermata in arrivo

            try
            {
                // Risolviamo il ViewModel (può stare fuori dal thread UI, alleggerendolo)
                var groupVM = Locator.Current.GetService<IPostazioneGroupViewModel>();

                if (groupVM != null)
                {
                    // Colleghiamo l'evento del nuovo ViewModel al ciclo di vita controllato della classe
                    groupVM.PostazioniToOperatori
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .Subscribe(async _ =>
                        {
                            GroupEnabled = false;
                            await GoToOperatoreGroup();
                        })
                        .DisposeWith(_navigationDisposables);

                    groupVM.PostazioniToSettori
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .Subscribe(async _ =>
                        {
                            GroupEnabled = false;
                            await GoToSettoreGroup();
                        })
                        .DisposeWith(_navigationDisposables);

                    groupVM.PostazioniToTariffe
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .Subscribe(async _ =>
                        {
                            GroupEnabled = false;
                            await GoToTariffaGroup();
                        })
                        .DisposeWith(_navigationDisposables);

                    // 3. NAVIGAZIONE SUL MAIN THREAD (Senza usare ScheduleAsync)
                    var tcs = new TaskCompletionSource();

                    RxSchedulers.MainThreadScheduler.Schedule(() =>
                    {
                        // Avviamo il comando sincrono del router di ReactiveUI 
                        // e usiamo la Subscribe per intercettare il completamento
                        Router.NavigateAndReset.Execute(groupVM)
                            .Subscribe(
                                _ => tcs.SetResult(), // Navigazione completata con successo
                                ex => tcs.SetException(ex) // Errore catturato
                            );
                    });

                    // Attendiamo in modo asincrono puro che la UI abbia finito lo switch visivo
                    await tcs.Task;
                }
                else
                {
                    Debug.WriteLine(">>> [ERROR] Impossibile risolvere IPostazioneGroupViewModel.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> [EXCEPTION] Errore durante la navigazione: {ex.Message}");
                throw;
            }
        }

        private async Task GoToSettoreGroup()
        {
            // 1. Puliamo subito i vecchi disposable (previene i memory leak delle vecchie view)
            _navigationDisposables.Clear();

            // 2. SOLUZIONE DOPPIO CLICK: Spegniamo subito l'interattività della view corrente
            GroupEnabled = false;
            await Task.Delay(200); // Assorbe il click "fantasma" hardware
            GroupEnabled = true;   // Riabilitiamo per la nuova schermata in arrivo

            try
            {
                // Risolviamo il ViewModel (può stare fuori dal thread UI, alleggerendolo)
                var groupVM = Locator.Current.GetService<ISettoreGroupViewModel>();

                if (groupVM != null)
                {
                    // Colleghiamo l'evento del nuovo ViewModel al ciclo di vita controllato della classe
                    groupVM.SettoriToOperatori
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .Subscribe(async _ =>
                        {
                            GroupEnabled = false;
                            await GoToOperatoreGroup();
                        })
                        .DisposeWith(_navigationDisposables);

                    groupVM.SettoriToPostazioni
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .Subscribe(async _ =>
                        {
                            GroupEnabled = false;
                            await GoToPostazioneGroup();
                        })
                        .DisposeWith(_navigationDisposables);

                    groupVM.SettoriToTariffe
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .Subscribe(async _ =>
                        {
                            GroupEnabled = false;
                            await GoToTariffaGroup();
                        })
                        .DisposeWith(_navigationDisposables);

                    // 3. NAVIGAZIONE SUL MAIN THREAD (Senza usare ScheduleAsync)
                    var tcs = new TaskCompletionSource();

                    RxSchedulers.MainThreadScheduler.Schedule(() =>
                    {
                        // Avviamo il comando sincrono del router di ReactiveUI 
                        // e usiamo la Subscribe per intercettare il completamento
                        Router.NavigateAndReset.Execute(groupVM)
                            .Subscribe(
                                _ => tcs.SetResult(), // Navigazione completata con successo
                                ex => tcs.SetException(ex) // Errore catturato
                            );
                    });

                    // Attendiamo in modo asincrono puro che la UI abbia finito lo switch visivo
                    await tcs.Task;
                }
                else
                {
                    Debug.WriteLine(">>> [ERROR] Impossibile risolvere IPostazioneGroupViewModel.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> [EXCEPTION] Errore durante la navigazione: {ex.Message}");
                throw;
            }
        }

        private async Task GoToTariffaGroup()
        {
            // 1. Puliamo subito i vecchi disposable (previene i memory leak delle vecchie view)
            _navigationDisposables.Clear();

            // 2. SOLUZIONE DOPPIO CLICK: Spegniamo subito l'interattività della view corrente
            GroupEnabled = false;
            await Task.Delay(200); // Assorbe il click "fantasma" hardware
            GroupEnabled = true;   // Riabilitiamo per la nuova schermata in arrivo

            try
            {
                // Risolviamo il ViewModel (può stare fuori dal thread UI, alleggerendolo)
                var groupVM = Locator.Current.GetService<ITariffaGroupViewModel>();

                if (groupVM != null)
                {
                    // Colleghiamo l'evento del nuovo ViewModel al ciclo di vita controllato della classe
                    groupVM.TariffaToOperatori
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .Subscribe(async _ =>
                        {
                            GroupEnabled = false;
                            await GoToOperatoreGroup();
                        })
                        .DisposeWith(_navigationDisposables);

                    groupVM.TariffaToPostazioni
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .Subscribe(async _ =>
                        {
                            GroupEnabled = false;
                            await GoToPostazioneGroup();
                        })
                        .DisposeWith(_navigationDisposables);

                    groupVM.TariffaToSettori
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .Subscribe(async _ =>
                        {
                            GroupEnabled = false;
                            await GoToSettoreGroup();
                        })
                        .DisposeWith(_navigationDisposables);

                    // 3. NAVIGAZIONE SUL MAIN THREAD (Senza usare ScheduleAsync)
                    var tcs = new TaskCompletionSource();

                    RxSchedulers.MainThreadScheduler.Schedule(() =>
                    {
                        // Avviamo il comando sincrono del router di ReactiveUI 
                        // e usiamo la Subscribe per intercettare il completamento
                        Router.NavigateAndReset.Execute(groupVM)
                            .Subscribe(
                                _ => tcs.SetResult(), // Navigazione completata con successo
                                ex => tcs.SetException(ex) // Errore catturato
                            );
                    });

                    // Attendiamo in modo asincrono puro che la UI abbia finito lo switch visivo
                    await tcs.Task;
                }
                else
                {
                    Debug.WriteLine(">>> [ERROR] Impossibile risolvere IPostazioneGroupViewModel.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> [EXCEPTION] Errore durante la navigazione: {ex.Message}");
                throw;
            }
        }

        private async Task GoToInput(IConfigurazioneCrudViewModel vm, int id = 0, int idRitorno = 0)
        {
            var tcs = new TaskCompletionSource();
            // 3. Risoluzione ViewModel e navigazione sul Main Thread
            RxSchedulers.MainThreadScheduler.Schedule(() =>
            {
                try
                {

                    if (vm != null)
                    {
                        if (id != 0) vm.SetIdDaModificare(id);
                        if (idRitorno != 0) vm.SetIdRitorno(idRitorno);

                        var disposables = new CompositeDisposable();
                        vm.InputEsc
                            .ObserveOn(RxSchedulers.MainThreadScheduler)
                            .Subscribe(_ =>
                            {
                                // Quando riceviamo il segnale di login riuscito, navighiamo al Menu
                                InputRouter?.NavigationStack.Clear();
                                GroupEnabled = true; // Riabilitiamo il gruppo per permettere nuove navigazioni
                            }).DisposeWith(disposables);
                        vm.InputBack
                            .ObserveOn(RxSchedulers.MainThreadScheduler)
                            .Take(1)
                            .Subscribe(value =>
                            {
                                try
                                {
                                    InputRouter.NavigateBack.Execute();
                                    AggiornaGridByInt(value);

                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Errore navigazione: {ex.Message}");
                                    _isClosing = false;
                                }
                                // Quando riceviamo il segnale di login riuscito, navighiamo al Menu
                                InputRouter?.NavigationStack.Clear();
                                GroupEnabled = true; // Riabilitiamo il gruppo per permettere nuove navigazioni
                            }).DisposeWith(disposables);

                        InputRouter.NavigateAndReset.Execute(vm)
                        .Subscribe(
                            _ => tcs.SetResult(),
                            ex => {
                                disposables.Dispose(); // In caso di errore svuotiamo le risorse
                                tcs.SetException(ex);
                            })
                        .DisposeWith(disposables);


                    }
                    else
                    {
                        Debug.WriteLine(">>> [ERROR] Il ViewModel passato a GoToInput è nullo.");
                        tcs.SetResult();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Errore durante la navigazione: {ex.Message}");
                    tcs.SetException(ex);
                }
            });

            await tcs.Task;
        }

    }
}
