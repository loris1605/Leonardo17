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
    public partial class ConfigurazioneViewModel() : ViewModelBase(), IConfigurazioneScreen, IConfigurazioneViewModel
    {
        // ---------------------------------------------------------------------
        // 1. Router Interni (Sub-Routing) e Dipendenze
        // ---------------------------------------------------------------------
        public RoutingState GroupRouter { get; } = new RoutingState();
        public RoutingState InputRouter { get; } = new RoutingState();

        // Espone il router principale richiesto dall'infrastruttura ReactiveUI
        public RoutingState Router => GroupRouter;

        //public new IScreen HostScreen => _host;

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
        private async Task GoToGroupGeneric<TViewModel>(Action<TViewModel> registerSubscriptions)
                where TViewModel : class, IRoutableViewModel
        {
            // 1. Pulizia dei vecchi disposable (previene i memory leak)
            _navigationDisposables.Clear();

            // 2. SOLUZIONE DOPPIO CLICK
            GroupEnabled = false;
            await Task.Delay(200);
            GroupEnabled = true;

            try
            {
                // Risoluzione dinamica del ViewModel dal Service Locator
                var groupVM = Locator.Current.GetService<TViewModel>();

                if (groupVM != null)
                {
                    // Eseguiamo il blocco di sottoscrizioni personalizzato passato come parametro
                    registerSubscriptions(groupVM);

                    // 3. NAVIGAZIONE SUL MAIN THREAD
                    var tcs = new TaskCompletionSource();

                    RxSchedulers.MainThreadScheduler.Schedule(() =>
                    {
                        Router.NavigateAndReset.Execute(groupVM)
                            .Subscribe(
                                _ => tcs.SetResult(),
                                ex => tcs.SetException(ex)
                            );
                    });

                    await tcs.Task;
                }
                else
                {
                    Debug.WriteLine($">>> [ERROR] Impossibile risolvere {typeof(TViewModel).Name}.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> [EXCEPTION] Errore durante la navigazione: {ex.Message}");
                throw;
            }
        }

        private Task GoToOperatoreGroup()
        {
            return GoToGroupGeneric<IOperatoreGroupViewModel>(groupVM =>
            {
                groupVM.OperatoreToPostazioni
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToPostazioneGroup(); })
                    .DisposeWith(_navigationDisposables);

                groupVM.OperatoreToSettori
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToSettoreGroup(); })
                    .DisposeWith(_navigationDisposables);

                groupVM.OperatoreToTariffe
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToTariffaGroup(); })
                    .DisposeWith(_navigationDisposables);

                groupVM.OperatoreToRientri
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToRientroGroup(); })
                    .DisposeWith(_navigationDisposables);

                groupVM.GroupToOperatoreAdd
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; 
                                            await GoToInput(Locator.Current.GetService<IOperatoreAddViewModel>()); })
                    .DisposeWith(_navigationDisposables);

                groupVM.GroupToOperatoreDel
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async id => { GroupEnabled = false; 
                                             await GoToInput(Locator.Current.GetService<IOperatoreDelViewModel>(), id); })
                    .DisposeWith(_navigationDisposables);

                groupVM.GroupToOperatoreUpd
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async id => { GroupEnabled = false; 
                                             await GoToInput(Locator.Current.GetService<IOperatoreUpdViewModel>(), id); })
                    .DisposeWith(_navigationDisposables);

                groupVM.GroupToPermessi
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async id => { GroupEnabled = false; 
                                             await GoToInput(Locator.Current.GetService<IPermessoViewModel>(), id); })
                    .DisposeWith(_navigationDisposables);
            });
        }

        private Task GoToPostazioneGroup()
        {
            return GoToGroupGeneric<IPostazioneGroupViewModel>(groupVM =>
            {
                groupVM.PostazioniToOperatori
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToOperatoreGroup(); })
                    .DisposeWith(_navigationDisposables);

                groupVM.PostazioniToSettori
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToSettoreGroup(); })
                    .DisposeWith(_navigationDisposables);

                groupVM.PostazioniToRientri
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToRientroGroup(); })
                    .DisposeWith(_navigationDisposables);

                groupVM.PostazioniToTariffe
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToTariffaGroup(); })
                    .DisposeWith(_navigationDisposables);

                groupVM.GroupToPostazioneAdd
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToInput(Locator.Current.GetService<IPostazioneAddViewModel>()); })
                    .DisposeWith(_navigationDisposables);

                groupVM.GroupToPostazioneDel
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async id => { GroupEnabled = false; await GoToInput(Locator.Current.GetService<IPostazioneDelViewModel>(), id); })
                    .DisposeWith(_navigationDisposables);

                groupVM.GroupToPostazioneUpd
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async id => { GroupEnabled = false; await GoToInput(Locator.Current.GetService<IPostazioneUpdViewModel>(), id); })
                    .DisposeWith(_navigationDisposables);

                groupVM.GroupToReparti
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async id => { GroupEnabled = false; await GoToInput(Locator.Current.GetService<IRepartoViewModel>(), id); })
                    .DisposeWith(_navigationDisposables);
            });
        }

        private Task GoToSettoreGroup()
        {
            return GoToGroupGeneric<ISettoreGroupViewModel>(groupVM =>
            {
                // 1. Transizioni verso gli altri macro-gruppi
                groupVM.SettoriToOperatori
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToOperatoreGroup(); })
                    .DisposeWith(_navigationDisposables);

                groupVM.SettoriToPostazioni
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToPostazioneGroup(); })
                    .DisposeWith(_navigationDisposables);

                groupVM.SettoriToTariffe
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToTariffaGroup(); })
                    .DisposeWith(_navigationDisposables);

                groupVM.SettoriToRientri
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToRientroGroup(); })
                    .DisposeWith(_navigationDisposables);

                // 2. Transizioni verso le maschere di input (Aggiunta)
                groupVM.GroupToSettoreAdd
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToInput(Locator.Current.GetService<ISettoreAddViewModel>()); })
                    .DisposeWith(_navigationDisposables);

                // 3. Transizioni verso le maschere di input con ID (Cancellazione, Modifica, Listino)
                groupVM.GroupToSettoreDel
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async id => { GroupEnabled = false; await GoToInput(Locator.Current.GetService<ISettoreDelViewModel>(), id); })
                    .DisposeWith(_navigationDisposables);

                groupVM.GroupToSettoreUpd
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async id => { GroupEnabled = false; await GoToInput(Locator.Current.GetService<ISettoreUpdViewModel>(), id); })
                    .DisposeWith(_navigationDisposables);

                groupVM.GroupToListino
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async id => { GroupEnabled = false; await GoToInput(Locator.Current.GetService<IListinoViewModel>(), id); })
                    .DisposeWith(_navigationDisposables);
            });
        }

        private Task GoToTariffaGroup()
        {
            return GoToGroupGeneric<ITariffaGroupViewModel>(groupVM =>
            {
                // 1. Transizioni verso gli altri macro-gruppi
                groupVM.TariffaToOperatori
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToOperatoreGroup(); })
                    .DisposeWith(_navigationDisposables);

                groupVM.TariffaToPostazioni
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToPostazioneGroup(); })
                    .DisposeWith(_navigationDisposables);

                groupVM.TariffaToSettori
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToSettoreGroup(); })
                    .DisposeWith(_navigationDisposables);

                groupVM.TariffaToRientri
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToRientroGroup(); })
                    .DisposeWith(_navigationDisposables);

                // 2. Transizioni verso le maschere di input (Aggiunta)
                groupVM.GroupToTariffaAdd
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToInput(Locator.Current.GetService<ITariffaAddViewModel>()); })
                    .DisposeWith(_navigationDisposables);

                // 3. Transizioni verso le maschere di input con ID (Cancellazione e Modifica)
                groupVM.GroupToTariffaDel
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async id => { GroupEnabled = false; await GoToInput(Locator.Current.GetService<ITariffaDelViewModel>(), id); })
                    .DisposeWith(_navigationDisposables);

                groupVM.GroupToTariffaUpd
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async id => { GroupEnabled = false; await GoToInput(Locator.Current.GetService<ITariffaUpdViewModel>(), id); })
                    .DisposeWith(_navigationDisposables);
            });
        }

        private Task GoToRientroGroup()
        {
            return GoToGroupGeneric<ITipoRientroGroupViewModel>(groupVM =>
            {
                // 1. Transizioni verso gli altri macro-gruppi
                groupVM.TipoRientroToOperatori
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToOperatoreGroup(); })
                    .DisposeWith(_navigationDisposables);

                groupVM.TipoRientroToPostazioni
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToPostazioneGroup(); })
                    .DisposeWith(_navigationDisposables);

                groupVM.TipoRientroToSettori
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToSettoreGroup(); })
                    .DisposeWith(_navigationDisposables);

                groupVM.TipoRientroToTariffe
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToTariffaGroup(); })
                    .DisposeWith(_navigationDisposables);

                // 2. Transizioni verso le maschere di input (Aggiunta)
                groupVM.GroupToTipoRientroAdd
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { GroupEnabled = false; await GoToInput(Locator.Current.GetService<ITipoRientroAddViewModel>()); })
                    .DisposeWith(_navigationDisposables);

                // 3. Transizioni verso le maschere di input con ID (Cancellazione e Modifica)
                groupVM.GroupToTipoRientroDel
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async id => { GroupEnabled = false; await GoToInput(Locator.Current.GetService<ITipoRientroDelViewModel>(), id); })
                    .DisposeWith(_navigationDisposables);

                groupVM.GroupToTipoRientroUpd
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async id => { GroupEnabled = false; await GoToInput(Locator.Current.GetService<ITipoRientroUpdViewModel>(), id); })
                    .DisposeWith(_navigationDisposables);
            });
        }

        private async Task GoToInput<TViewModel>(TViewModel vm, int id = 0, int idRitorno = 0)
                        where TViewModel : class, IConfigurazioneCrudViewModel
        {
            if (vm == null)
            {
                Debug.WriteLine($">>> [ERROR] Il ViewModel {typeof(TViewModel).Name} passato a GoToInput è nullo.");
                return;
            }

            // Configurazione dei parametri sul ViewModel prima di entrare nel Thread UI
            if (id != 0) vm.SetIdDaModificare(id);
            if (idRitorno != 0) vm.SetIdRitorno(idRitorno);

            var tcs = new TaskCompletionSource();

            // Creiamo il contenitore per i disposable di QUESTA specifica sessione di input
            var localDisposables = new CompositeDisposable();

            RxSchedulers.MainThreadScheduler.Schedule(() =>
            {
                try
                {
                    // Gestione ESC
                    vm.InputEsc
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .Subscribe(_ =>
                        {
                            InputRouter?.NavigationStack.Clear();
                            GroupEnabled = true;
                            localDisposables.Dispose(); // Libera la memoria alla chiusura
                        })
                        .DisposeWith(localDisposables);

                    // Gestione BACK
                    vm.InputBack
                        .ObserveOn(RxSchedulers.MainThreadScheduler)
                        .Take(1)
                        .Subscribe(value =>
                        {
                            try
                            {
                                InputRouter?.NavigateBack.Execute();
                                AggiornaGridByInt(value);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Errore navigazione back: {ex.Message}");
                                _isClosing = false;
                            }

                            InputRouter?.NavigationStack.Clear();
                            GroupEnabled = true;
                            localDisposables.Dispose(); // Libera la memoria alla chiusura
                        })
                        .DisposeWith(localDisposables);

                    // Esecuzione Navigazione
                    InputRouter.NavigateAndReset.Execute(vm)
                        .Subscribe(
                            _ => tcs.SetResult(),
                            ex =>
                            {
                                localDisposables.Dispose(); // Libera in caso di errore immediato
                                tcs.SetException(ex);
                            }
                        )
                        .DisposeWith(localDisposables);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Errore durante la configurazione UI di GoToInput: {ex.Message}");
                    localDisposables.Dispose();
                    tcs.SetException(ex);
                }
            });

            await tcs.Task;
        }

      

    }
}
