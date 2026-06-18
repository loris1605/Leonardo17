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

namespace Soci.ViewModels
{
    public interface ISociScreen : IScreen
    {
        RoutingState GroupRouter { get; }
        RoutingState InputRouter { get; }
        bool GroupEnabled { get; set; }

        void AggiornaGridByInt(int id);
        void AggiornaGridByObject(object model);
    }

    public interface ISociCrudViewModel : IRoutableViewModel
    {
        void SetIdDaModificare(int id);
        void SetIdRitorno(int id);
        IObservable<Unit> InputEsc { get; }
        IObservable<int> InputBack { get; }
    }


    public partial class SociViewModel(IScreen host) : ViewModelBase(host), ISociScreen, ISociViewModel
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
        private readonly Subject<Unit> _sociToMenu = new();
        public IObservable<Unit> SociToMenu => _sociToMenu.AsObservable();

        // ---------------------------------------------------------------------
        // 3. Ciclo di Vita (Override dei Metodi Virtuali della Base)
        // ---------------------------------------------------------------------

        protected override void OnFinalDestruction()
        {
            // Svuotiamo gli stack di navigazione dei router interni per liberare le View collegate
            GroupRouter?.NavigationStack.Clear();
            InputRouter?.NavigationStack.Clear();
            _host = null;

            base.OnFinalDestruction();
        }

        protected override async Task OnLoading() => await GoToPersonGroup();
        protected override async Task OnSaving() => await Task.CompletedTask;
        protected override async Task OnEsc()
        {
            _isClosing = true;
            _sociToMenu.OnNext(Unit.Default);
            _sociToMenu.OnCompleted(); // Chiude il canale per sempre, prevenendo ulteriori notifiche

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

    public partial class SociViewModel
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

    public partial class SociViewModel
    {
        private async Task GoToPersonGroup()
        {

            var tcs = new TaskCompletionSource();

            // 3. Risoluzione ViewModel e navigazione sul Main Thread
            RxSchedulers.MainThreadScheduler.Schedule(() =>
            {
                try
                {
                    // Nascendo qui dentro, il costruttore del LoginViewModel 
                    // viene eseguito sul thread UI, azzerando l'errore Cross-Thread!
                    var groupVM = Locator.Current.GetService<IPersonGroupViewModel>();

                    if (groupVM != null)
                    {
                        var personDisposables = new CompositeDisposable();

                        SetEvents(groupVM, personDisposables).Wait();


                        // Eseguiamo la navigazione e segnaliamo il completamento del Task
                        Router.NavigateAndReset.Execute(groupVM)
                        .Subscribe(
                            _ => tcs.SetResult(),
                            ex => {
                                personDisposables.Dispose(); // In caso di errore svuotiamo le risorse
                                tcs.SetException(ex);
                            })
                        .DisposeWith(personDisposables);
                    }
                    else
                    {
                        Debug.WriteLine(">>> [ERROR] Impossibile risolvere IPersonGroupViewModel.");
                        tcs.SetResult();
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }

            });

            await tcs.Task;
        }

        private async Task GoToInput(ISociCrudViewModel vm, int id = 0, int idRitorno = 0)
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

        private async Task GoToSearch()
        {
            var tcs = new TaskCompletionSource();
            // 3. Risoluzione ViewModel e navigazione sul Main Thread
            RxSchedulers.MainThreadScheduler.Schedule(() =>
            {

                try
                {

                    var vm = Locator.Current.GetService<IPersonSearchViewModel>();

                    if (vm != null)
                    {
                        

                        var disposables = new CompositeDisposable();
                        vm.InputEsc
                            .ObserveOn(RxSchedulers.MainThreadScheduler)
                            .Subscribe(_ =>
                            {
                                // Quando riceviamo il segnale di login riuscito, navighiamo al Menu
                                InputRouter?.NavigationStack.Clear();
                                GroupEnabled = true; // Riabilitiamo il gruppo per permettere nuove navigazioni
                            }).DisposeWith(disposables);
                        vm.InputBackFiltered
                            .ObserveOn(RxSchedulers.MainThreadScheduler)
                            .Take(1)
                            .Subscribe(value =>
                            {
                                try
                                {
                                    InputRouter.NavigateBack.Execute();
                                    AggiornaGridByObject(value);

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

        private async Task SetEvents(IPersonGroupViewModel personVM, CompositeDisposable disposables)
        {
            personVM.GroupToPersonAdd
                            .ObserveOn(RxSchedulers.MainThreadScheduler)
                            .Subscribe(async _ =>
                            {
                                // Quando riceviamo il segnale di richiesta Add da parte del gruppo, navighiamo alla schermata di input
                                GroupEnabled = false; // Disabilitiamo il gruppo per evitare navigazioni multiple
                                await GoToInput(Locator.Current.GetService<IPersonAddViewModel>());
                            }).DisposeWith(disposables);

            personVM.GroupToPersonSearch
                            .ObserveOn(RxSchedulers.MainThreadScheduler)
                            .Subscribe(async _ =>
                            {
                                // Quando riceviamo il segnale di richiesta Add da parte del gruppo, navighiamo alla schermata di input
                                GroupEnabled = false; // Disabilitiamo il gruppo per evitare navigazioni multiple
                                await GoToSearch();
                            }).DisposeWith(disposables);

            personVM.GroupToPersonDel
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(async id =>
                {
                    // Quando riceviamo il segnale di richiesta Del da parte del gruppo, navighiamo alla schermata di input
                    GroupEnabled = false; // Disabilitiamo il gruppo per evitare navigazioni multiple
                    await GoToInput(Locator.Current.GetService<IPersonDelViewModel>(), id);
                }).DisposeWith(disposables);

            personVM.GroupToPersonUpd
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(async id =>
                {
                    // Quando riceviamo il segnale di richiesta Upd da parte del gruppo, navighiamo alla schermata di input
                    GroupEnabled = false; // Disabilitiamo il gruppo per evitare navigazioni multiple
                    await GoToInput(Locator.Current.GetService<IPersonUpdViewModel>(), id);
                }).DisposeWith(disposables);

            personVM.GroupToCodiceSocioAdd
                            .ObserveOn(RxSchedulers.MainThreadScheduler)
                            .Subscribe(async id =>
                            {
                                // Quando riceviamo il segnale di richiesta Add da parte del gruppo, navighiamo alla schermata di input
                                GroupEnabled = false; // Disabilitiamo il gruppo per evitare navigazioni multiple
                                await GoToInput(Locator.Current.GetService<ICodiceSocioAddViewModel>(), id);
                            }).DisposeWith(disposables);

            personVM.GroupToCodiceSocioDel
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(async id =>
                {
                    // Quando riceviamo il segnale di richiesta Del da parte del gruppo, navighiamo alla schermata di input
                    GroupEnabled = false; // Disabilitiamo il gruppo per evitare navigazioni multiple
                    await GoToInput(Locator.Current.GetService<ICodiceSocioDelViewModel>(), id);
                }).DisposeWith(disposables);

            personVM.GroupToCodiceSocioUpd
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(async dati =>
                {
                    // Quando riceviamo il segnale di richiesta Upd da parte del gruppo, navighiamo alla schermata di input
                    GroupEnabled = false; // Disabilitiamo il gruppo per evitare navigazioni multiple
                    await GoToInput(Locator.Current.GetService<ICodiceSocioUpdViewModel>(), dati.id, dati.idRitorno);
                }).DisposeWith(disposables);

            personVM.GroupToTesseraAdd
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(async dati =>
                {
                    // Quando riceviamo il segnale di richiesta Add da parte del gruppo, navighiamo alla schermata di input
                    GroupEnabled = false; // Disabilitiamo il gruppo per evitare navigazioni multiple
                    await GoToInput(Locator.Current.GetService<ITesseraAddViewModel>(), dati.id, dati.idRitorno);
                }).DisposeWith(disposables);

            personVM.GroupToTesseraDel
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(async dati =>
                {
                    // Quando riceviamo il segnale di richiesta Del da parte del gruppo, navighiamo alla schermata di input
                    GroupEnabled = false; // Disabilitiamo il gruppo per evitare navigazioni multiple
                    await GoToInput(Locator.Current.GetService<ITesseraDelViewModel>(), dati.id, dati.idRitorno);
                }).DisposeWith(disposables);

            personVM.GroupToTesseraUpd
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(async dati =>
                {
                    // Quando riceviamo il segnale di richiesta Upd da parte del gruppo, navighiamo alla schermata di input
                    GroupEnabled = false; // Disabilitiamo il gruppo per evitare navigazioni multiple
                    await GoToInput(Locator.Current.GetService<ITesseraUpdViewModel>(), dati.id, dati.idRitorno);
                }).DisposeWith(disposables);


            await Task.CompletedTask;
        }
    }
        
}

