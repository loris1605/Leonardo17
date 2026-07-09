using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using ViewModels;

namespace Cassa.ViewModels
{
    public interface ICassaViewModel
    {

    }

    public partial class CassaViewModel : ViewModelBase, ICassaViewModel
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
        public RoutingState GroupRouter { get; } = new RoutingState();
        //public RoutingState InputRouter { get; } = new RoutingState();

        // Espone il router principale richiesto dall'infrastruttura ReactiveUI
        public RoutingState Router => GroupRouter;

        protected override void OnFinalDestruction()
        {
            // Svuotiamo gli stack di navigazione dei router interni per liberare le View collegate
            _navigationDisposables.Dispose();
            GroupRouter?.NavigationStack.Clear();
            
            base.OnFinalDestruction();
        }

        protected override async Task OnLoading() => await GoToPostazione();

        private async Task GoToPostazione()
        {
            await Task.Delay(100); // Simula un ritardo per la navigazione
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

            // 2. SOLUZIONE DOPPIO CLICK
            await Task.Delay(200);
            
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
    }
}
