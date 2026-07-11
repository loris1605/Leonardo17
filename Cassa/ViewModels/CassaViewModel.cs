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

    public partial class CassaViewModel() : ViewModelBase(), ICassaScreen, ICassaViewModel
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

        protected override Task OnLoading()
        {
            // Avviamo la navigazione sul thread della UI ma senza fare l'await del Task 
            // all'interno del ciclo di vita sincrono della classe base
            RxSchedulers.MainThreadScheduler.Schedule(async () =>
            {
                await GoToPostazione();
            });

            return Task.CompletedTask;
        }



        private Task GoToPostazione()
        {
            return GoToPageGeneric<ICassaPostazioneViewModel>(pageVM =>
            {
                pageVM.PostazioneToMenu
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    // Sostituiamo il Subscribe(async...) con SelectMany per gestire il Task in modo reattivo
                    .SelectMany(_ => Observable.FromAsync(() => GoToMenu()))
                    .Subscribe()
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

        private async Task GoToPageGeneric<TViewModel>(Action<TViewModel> registerSubscriptions)
        where TViewModel : class, IRoutableViewModel
        {
            // 1. Pulizia dei vecchi disposable (previene i memory leak)
            _navigationDisposables.Clear();

            // 2. SOLUZIONE DOPPIO CLICK (Resta asincrona senza bloccare il thread)
            await Task.Delay(200);

            try
            {
                // Risoluzione dinamica del ViewModel dal Service Locator
                var groupVM = Locator.Current.GetService<TViewModel>();

                if (groupVM != null)
                {
                    // Eseguiamo il blocco di sottoscrizioni personalizzato passato come parametro
                    registerSubscriptions(groupVM);

                    // 3. NAVIGAZIONE SUL MAIN THREAD (Senza TaskCompletionSource sincrono)
                    RxSchedulers.MainThreadScheduler.Schedule(() =>
                    {
                        // Avviamo la navigazione. ReactiveUI gestirà internamente il cambio di View 
                        // in modo asincrono sul thread della UI senza che questo thread debba attendere.
                        Router.NavigateAndReset.Execute(groupVM)
                            .Subscribe(
                                _ => Debug.WriteLine($">>> Navigazione completata verso {typeof(TViewModel).Name}"),
                                ex => Debug.WriteLine($">>> [ERROR] Errore durante NavigateAndReset: {ex.Message}")
                            );
                    });

                    // Restituiamo il controllo al chiamante immediatamente. La UI ora è libera di aggiornarsi.
                    await Task.CompletedTask;
                }
                else
                {
                    Debug.WriteLine($">>> [ERROR] Impossibile risolvere {typeof(TViewModel).Name}.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> [EXCEPTION] Errore durante l'inizializzazione della navigazione: {ex.Message}");
                throw;
            }
        }


    }
}
