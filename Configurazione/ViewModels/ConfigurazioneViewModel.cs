using Contracts;
using ReactiveUI;
using Splat;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
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

            var tcs = new TaskCompletionSource();

            // 3. Risoluzione ViewModel e navigazione sul Main Thread
            RxSchedulers.MainThreadScheduler.Schedule(() =>
            {
                try
                {
                    // Nascendo qui dentro, il costruttore del LoginViewModel 
                    // viene eseguito sul thread UI, azzerando l'errore Cross-Thread!
                    //var groupVM = Locator.Current.GetService<IPersonGroupViewModel>();

                    //if (groupVM != null)
                    //{
                    //    var personDisposables = new CompositeDisposable();

                    //    SetEvents(groupVM, personDisposables).Wait();


                        // Eseguiamo la navigazione e segnaliamo il completamento del Task
                        //Router.NavigateAndReset.Execute(groupVM)
                        //.Subscribe(
                        //    _ => tcs.SetResult(),
                        //    ex => {
                        //        personDisposables.Dispose(); // In caso di errore svuotiamo le risorse
                        //        tcs.SetException(ex);
                        //    })
                        //.DisposeWith(personDisposables);
                    //}
                    //else
                    //{
                    //    Debug.WriteLine(">>> [ERROR] Impossibile risolvere IPersonGroupViewModel.");
                    //    tcs.SetResult();
                    //}
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }

            });

            await tcs.Task;
        }
    }
}
