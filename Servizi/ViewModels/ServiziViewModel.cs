using Contracts;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using ViewModels;

namespace Servizi.ViewModels
{
    public interface IServiziScreen : IScreen
    {
        RoutingState GroupRouter { get; }
        RoutingState InputRouter { get; }
        bool GroupEnabled { get; set; }

        void AggiornaGridByInt(int id);

    }

    public interface IServiziCrudViewModel : IRoutableViewModel
    {
        void SetIdDaModificare(int id);
        void SetIdRitorno(int id);
        IObservable<Unit> InputEsc { get; }
        IObservable<int> InputBack { get; }
    }

    public partial class ServiziViewModel() : ViewModelBase(), IServiziScreen, IServiziViewModel
    {
        // ---------------------------------------------------------------------
        // 1. Router Interni (Sub-Routing) e Dipendenze
        // ---------------------------------------------------------------------
        public RoutingState GroupRouter { get; } = new RoutingState();
        public RoutingState InputRouter { get; } = new RoutingState();

        // Espone il router principale richiesto dall'infrastruttura ReactiveUI
        public RoutingState Router => GroupRouter;

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
        private readonly Subject<Unit> _serviziToMenu = new();
        public IObservable<Unit> ServiziToMenu => _serviziToMenu.AsObservable();

        private readonly CompositeDisposable _navigationDisposables = [];

        protected override void OnFinalDestruction()
        {
            // Svuotiamo gli stack di navigazione dei router interni per liberare le View collegate
            _navigationDisposables.Dispose();
            GroupRouter?.NavigationStack.Clear();
            InputRouter?.NavigationStack.Clear();


            base.OnFinalDestruction();
        }

        protected override async Task OnLoading() => await GoToAbbonamentoGroup();
        protected override async Task OnSaving() => await Task.CompletedTask;
        protected override async Task OnEsc()
        {
            _isClosing = true;
            _serviziToMenu.OnNext(Unit.Default);
            _serviziToMenu.OnCompleted(); // Chiude il canale per sempre, prevenendo ulteriori notifiche

            await Task.CompletedTask;

        }
    }


    public partial class ServiziViewModel
    {
        #region GroupEnabled

        private bool _groupenabled = true;
        public bool GroupEnabled
        {
            get => _groupenabled;
            set => this.RaiseAndSetIfChanged(ref _groupenabled, value);
        }

        #endregion

        public void AggiornaGridByInt(int id)
        {
            if (GroupRouter.GetCurrentViewModel() is IGroupViewModelBase groupVm)
            {
                // Passiamo l'ID al metodo di caricamento della lista
                groupVm.CaricaDataSource(id);
            }
        } 

    }

    public partial class ServiziViewModel
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

        private Task GoToAbbonamentoGroup()
        {

            return GoToGroupGeneric<IAbbonamentoGroupViewModel>(groupVM => { });
            //{


            //groupVM.OperatoreToPostazioni
            //    .ObserveOn(RxSchedulers.MainThreadScheduler)
            //    .Subscribe(async _ => { GroupEnabled = false; await GoToPostazioneGroup(); })
            //    .DisposeWith(_navigationDisposables);

            //groupVM.OperatoreToSettori
            //    .ObserveOn(RxSchedulers.MainThreadScheduler)
            //    .Subscribe(async _ => { GroupEnabled = false; await GoToSettoreGroup(); })
            //    .DisposeWith(_navigationDisposables);

            //groupVM.OperatoreToTariffe
            //    .ObserveOn(RxSchedulers.MainThreadScheduler)
            //    .Subscribe(async _ => { GroupEnabled = false; await GoToTariffaGroup(); })
            //    .DisposeWith(_navigationDisposables);

            //groupVM.OperatoreToRientri
            //    .ObserveOn(RxSchedulers.MainThreadScheduler)
            //    .Subscribe(async _ => { GroupEnabled = false; await GoToRientroGroup(); })
            //    .DisposeWith(_navigationDisposables);

            //groupVM.GroupToOperatoreAdd
            //    .ObserveOn(RxSchedulers.MainThreadScheduler)
            //    .Subscribe(async _ =>
            //    {
            //        GroupEnabled = false;
            //        await GoToInput(Locator.Current.GetService<IOperatoreAddViewModel>());
            //    })
            //    .DisposeWith(_navigationDisposables);

            //groupVM.GroupToOperatoreDel
            //    .ObserveOn(RxSchedulers.MainThreadScheduler)
            //    .Subscribe(async id =>
            //    {
            //        GroupEnabled = false;
            //        await GoToInput(Locator.Current.GetService<IOperatoreDelViewModel>(), id);
            //    })
            //    .DisposeWith(_navigationDisposables);

            //groupVM.GroupToOperatoreUpd
            //    .ObserveOn(RxSchedulers.MainThreadScheduler)
            //    .Subscribe(async id =>
            //    {
            //        GroupEnabled = false;
            //        await GoToInput(Locator.Current.GetService<IOperatoreUpdViewModel>(), id);
            //    })
            //    .DisposeWith(_navigationDisposables);

            //groupVM.GroupToPermessi
            //    .ObserveOn(RxSchedulers.MainThreadScheduler)
            //    .Subscribe(async id =>
            //    {
            //        GroupEnabled = false;
            //        await GoToInput(Locator.Current.GetService<IPermessoViewModel>(), id);
            //    })
            //    .DisposeWith(_navigationDisposables);
            //});

            
        }
    }
}
