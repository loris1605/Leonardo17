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
using System.Reactive.Threading.Tasks;
using ViewModels;

namespace Cassa.ViewModels
{

    public interface ICassaScreen : IScreen
    {
        RoutingState CassaRouter { get; }
        RoutingState SettingsRouter { get; }
        
    }

    public partial class CassaViewModel : ViewModelBase, ICassaViewModel, ICassaScreen
    {
        protected override IObservable<bool> IsAnythingExecuting =>
            Observable.CombineLatest(
                // 1. Comandi base ereditati
                this.WhenAnyObservable(x => x.LoadCommand.IsExecuting).StartWith(false),
                this.WhenAnyObservable(x => x.SaveCommand.IsExecuting).StartWith(false),
                this.WhenAnyObservable(x => x.EscPressedCommand.IsExecuting).StartWith(false),
                // 2. Monitoraggio delle esecuzioni dei router (Navigazioni in corso)
               
                // Se qualunque operazione o cambio pagina è attivo, blocca la UI
                (l, s, e) => l || s || e )
            .DistinctUntilChanged();

        private readonly CompositeDisposable _navigationDisposables = [];

        // ---------------------------------------------------------------------
        // 1. Router Interni (Sub-Routing) e Dipendenze
        // ---------------------------------------------------------------------
        public RoutingState CassaRouter { get; } = new RoutingState();
        public RoutingState SettingsRouter { get; } = new RoutingState();

        // Espone il router principale richiesto dall'infrastruttura ReactiveUI
        public RoutingState Router => CassaRouter;  
        protected override void OnFinalDestruction()
        {
            // Svuotiamo gli stack di navigazione dei router interni per liberare le View collegate
            _navigationDisposables.Dispose();
                        
            base.OnFinalDestruction();
        }

        protected override async Task OnLoading() => await GoToPostazione();

        

        private Task GoToPostazione()
        {
            return GoToPage<ICassaPostazioneViewModel>(pageVM =>
            {
                pageVM.PostazioneToMenu
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Subscribe(async _ => { await GoToMenu(); })
                    .DisposeWith(_navigationDisposables);
            });

        }

        private async Task GoToMenu()
        {
            // Notifica l'esterno facendolo girare nel prossimo ciclo del MainThread,
            // dando tempo alla pagina corrente di completare lo smontaggio della View in modo pulito.
            RxSchedulers.MainThreadScheduler.Schedule(() =>
            {
                _cassaToMenu.OnNext(Unit.Default);
            });

            await Task.CompletedTask;
        }


    }

    public partial class CassaViewModel
    {
        // 1. Aggiungi questo Subject per notificare l'esterno
        private readonly Subject<Unit> _cassaToMenu = new();
        public IObservable<Unit> CassaToMenu => _cassaToMenu.AsObservable();

        private async Task GoToPage<TViewModel>(Action<TViewModel> registerSubscriptions)
        where TViewModel : class, IRoutableViewModel
        {
            _navigationDisposables.Clear();

            await Task.Delay(200); // Soluzione doppio click

            try
            {
                var pageVM = Locator.Current.GetService<TViewModel>();

                if (pageVM != null)
                {
                    registerSubscriptions(pageVM);

                    var tcs = new TaskCompletionSource();

                    await Router.NavigateAndReset.Execute(pageVM)
                        .Catch<IRoutableViewModel, Exception>(ex =>
                        {
                            Debug.WriteLine($">>> [ROUTER EXCEPTION] Errore controllato nella pipeline: {ex.Message}");
                            return Observable.Empty<IRoutableViewModel>();
                        })
                        .ToTask(); // Richiede 'using System.Reactive.Threading.Tasks;'

                }
                else
                {
                    Debug.WriteLine($">>> [ERROR] Impossibile risolvere {typeof(TViewModel).Name}.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> [EXCEPTION] Errore intercettato durante la navigazione: {ex.Message}");
                // Non rilanciare l'eccezione con "throw;" se vuoi evitare che uccida l'applicazione, 
                // gestiscila o loggala.
            }
        }

    }
}
