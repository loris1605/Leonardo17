using Contracts;
using ReactiveUI;
using Splat;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ViewModels;

namespace Cassa.ViewModels
{
    public interface ICassaScreen : IScreen
    {
        RoutingState CassaRouter { get; }
        RoutingState SettingsRouter { get; }
    }

    public partial class CassaViewModel : ViewModelBase, ICassaScreen, ICassaViewModel
    {
        protected override IObservable<bool> IsAnythingExecuting =>
            Observable.CombineLatest(
                this.WhenAnyObservable(x => x.LoadCommand.IsExecuting).StartWith(false),
                this.WhenAnyObservable(x => x.SaveCommand.IsExecuting).StartWith(false),
                this.WhenAnyObservable(x => x.EscPressedCommand.IsExecuting).StartWith(false),
                (l, s, e) => l || s || e
            )
            .DistinctUntilChanged();

        private readonly CompositeDisposable _navigationDisposables = new();
        private int _cassaPostazioneId;

        public RoutingState CassaRouter { get; } = new RoutingState();
        public RoutingState SettingsRouter { get; } = new RoutingState();
        public RoutingState Router => CassaRouter;

        protected override void OnFinalDestruction()
        {
            _navigationDisposables.Dispose();
            base.OnFinalDestruction();
        }

        public void SetPostazioneId(int id) => _cassaPostazioneId = id;

        protected override Task OnLoading()
        {
            // Avvia la navigazione sul MainThread (non blocca il caller)
            RxSchedulers.MainThreadScheduler.Schedule(async () =>
            {
                await GoToPostazione(_cassaPostazioneId, string.Empty);
            });

            return Task.CompletedTask;
        }

        private Task GoToPostazione(int postazioneId, string posizione)
        {
            return GoToPageGeneric<ICassaPostazioneViewModel>(pageVM =>
            {
                // Nota: ora registerSubscriptions verrà chiamato DOPO che la navigazione è completata
                // qui non eseguiamo più SetPostazioneId/ApriScheda immediatamente
                // (vedi implementazione di GoToPageGeneric)
                pageVM.SetPostazioneId(postazioneId);
                pageVM.SetPosizione(posizione);
                pageVM.ApriScheda();

                var navStream = Observable.Merge(
                    pageVM.PostazioneToMenu
                        .SelectMany(_ => Observable.FromAsync(() => GoToMenu())),
                    pageVM.PostazioneToEntraSocio
                        .SelectMany(tuple => Observable.FromAsync(() => GoToEntraSocio(tuple.postazioneId, tuple.posizione))),
                    pageVM.PostazioneToListaSoci
                        .SelectMany(tuple => Observable.FromAsync(() => GoToListaSoci(tuple.postazioneId, tuple.posizione)))
                )
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(
                    _ => { },
                    ex => Console.Error.WriteLine($"Navigation error: {ex}")
                );

                navStream.DisposeWith(_navigationDisposables);
            });
        }

        private async Task GoToMenu()
        {
            RxSchedulers.MainThreadScheduler.Schedule(() => _cassaToMenu.OnNext(Unit.Default));
            await Task.CompletedTask;
        }

        private async Task GoToEntraSocio(int postazioneId, string posizione)
        {
            await GoToPageGeneric<IEntraSocioViewModel>(pageVM =>
            {
                pageVM.SetPostazioneId(postazioneId);
                pageVM.SetPosizione(posizione);

                pageVM.EntraSocioToPostazione
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .SelectMany(tuple => Observable.FromAsync(() => GoToPostazione(tuple.postazioneId, tuple.posizione)))
                    .Subscribe()
                    .DisposeWith(_navigationDisposables);
            });
        }

        private async Task GoToListaSoci(int postazioneId, string posizione)
        {
            await GoToPageGeneric<ICassaListaSociViewModel>(pageVM =>
            {
                pageVM.SetPostazioneId(postazioneId);
                pageVM.SetPosizione(posizione);

                pageVM.ListaSociToPostazione
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .SelectMany(tuple => Observable.FromAsync(() => GoToPostazione(tuple.postazioneId, tuple.posizione)))
                    .Subscribe()
                    .DisposeWith(_navigationDisposables);
            });
        }

        // notificatore esterno
        private readonly Subject<Unit> _cassaToMenu = new();
        public IObservable<Unit> CassaToMenu => _cassaToMenu.AsObservable();

        private async Task GoToPageGeneric<TViewModel>(Action<TViewModel> registerSubscriptions)
            where TViewModel : class, IRoutableViewModel
        {
            _navigationDisposables.Clear();
            await Task.Delay(200); // debounce doppio click

            try
            {
                var groupVM = Locator.Current.GetService<TViewModel>();
                if (groupVM == null)
                {
                    Debug.WriteLine($">>> [ERROR] Impossibile risolvere {typeof(TViewModel).Name}.");
                    return;
                }

                // SerialDisposable per la sottoscrizione di navigazione
                var navSerial = new SerialDisposable();
                _navigationDisposables.Add(navSerial);

                // Avvia la navigazione sul MainThread e registra le sottoscrizioni solo dopo il completamento
                RxSchedulers.MainThreadScheduler.Schedule(() =>
                {
                    navSerial.Disposable = Router.NavigateAndReset.Execute(groupVM)
                        .Subscribe(
                            _ => Debug.WriteLine($">>> Navigazione in corso verso {typeof(TViewModel).Name}"),
                            ex => Debug.WriteLine($">>> [ERROR] Errore durante NavigateAndReset: {ex.Message}"),
                            () =>
                            {
                                // Registrazioni che dipendono dalla pagina ora vengono fatte sul MainThread
                                RxSchedulers.MainThreadScheduler.Schedule(() => registerSubscriptions(groupVM));
                            });
                });

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> [EXCEPTION] Errore durante l'inizializzazione della navigazione: {ex.Message}");
                throw;
            }
        }
    }
}
