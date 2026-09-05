using Cassa.Core.Repository;
using Cassa.ViewModels.Map;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ViewModels;

namespace Cassa.ViewModels
{
    public interface ISchedaContoViewModel : IRoutableViewModel
    {
        // Define any properties or methods that the SchedaContoViewModel should implement
    }

    public class SchedaContoViewModel : ViewModelBase, ISchedaContoViewModel
    {
        private readonly CompositeDisposable _disposables = new();

        private readonly ICassaSchedaContoRepository _cassaSchedaContoRepository;
        private int _schedaId;

        public SchedaContoViewModel(ICassaSchedaContoRepository cassaSchedaContoRepository)
        {
            _cassaSchedaContoRepository = cassaSchedaContoRepository ?? throw new ArgumentNullException(nameof(cassaSchedaContoRepository));
        }

        protected override IObservable<bool> IsAnythingExecuting =>
            Observable.CombineLatest(
            [
                base.IsAnythingExecuting ?? Observable.Return(false),

                //this.WhenAnyValue(vm => vm.EntraSocioCommand)
                //    .Select(cmd => cmd?.IsExecuting ?? Observable.Return(false))
                //    .Switch(),

                //this.WhenAnyValue(vm => vm.EsceSocioCommand)
                //    .Select(cmd => cmd?.IsExecuting ?? Observable.Return(false))
                //    .Switch(),

                //this.WhenAnyValue(vm => vm.ListaSociCommand)
                //    .Select(cmd => cmd?.IsExecuting ?? Observable.Return(false))
                //    .Switch(),

                //this.WhenAnyValue(vm => vm.PosizioneEnterCommand)
                //    .Select(cmd => cmd?.IsExecuting ?? Observable.Return(false))
                //    .Switch(),

                //this.WhenAnyValue(vm => vm.PosizioneEscCommand)
                //    .Select(cmd => cmd?.IsExecuting ?? Observable.Return(false))
                //    .Switch()
            ], results => results.Any(x => x))
            .DistinctUntilChanged();

        protected override void OnFinalDestruction()
        {
            // Dispose of all subscriptions and subjects
            _disposables.Dispose();
            base.OnFinalDestruction();
        }


        #region Bindings

        private List<CassaSchedaContoMap> schedaContoMaps = new();
        public List<CassaSchedaContoMap> SchedaContoMaps
        {
            get => schedaContoMaps;
            set => this.RaiseAndSetIfChanged(ref schedaContoMaps, value);
        }

        private CassaSchedaContoMap selectedSchedaContoMap;
        public CassaSchedaContoMap SelectedSchedaContoMap
        {
            get => selectedSchedaContoMap;
            set => this.RaiseAndSetIfChanged(ref selectedSchedaContoMap, value);
        }

        #endregion

    }
}
