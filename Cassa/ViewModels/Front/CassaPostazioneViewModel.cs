using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ViewModels;

namespace Cassa.ViewModels
{
    public interface ICassaPostazioneViewModel : IRoutableViewModel
    {
        // Define any properties or methods that the CassaPostazioneViewModel should implement
        IObservable<Unit> PostazioneToMenu { get; }
    }

    public partial class CassaPostazioneViewModel : ViewModelBase, ICassaPostazioneViewModel
    {
        public ReactiveCommand<Unit, Unit> NuovoIngressoCommand { get; }



        protected override IObservable<bool> IsAnythingExecuting =>
        Observable.CombineLatest(
            // 1. Comandi ereditati dalla classe base
            this.WhenAnyObservable(x => x.LoadCommand.IsExecuting).StartWith(false),
            this.WhenAnyObservable(x => x.SaveCommand.IsExecuting).StartWith(false),
            this.WhenAnyObservable(x => x.EscPressedCommand.IsExecuting).StartWith(false),
            // 2. Comandi specifici osservati in modo sicuro direttamente tramite le loro proprietà
            this.WhenAnyObservable(
                x => x.NuovoIngressoCommand.IsExecuting
                //x => x.SettoriCommand.IsExecuting,
                //x => x.TariffeCommand.IsExecuting,
                //x => x.PermessiCommand.IsExecuting,
                //x => x.RientriCommand.IsExecuting
            ).StartWith(false),
            // Se anche uno solo è in esecuzione, restituisce true
            (baseLoad, baseSave, baseEsc, localExec) => baseLoad || baseSave || baseEsc || localExec)
        .DistinctUntilChanged();

        public CassaPostazioneViewModel() : base(null)
        {
            NuovoIngressoCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                // Logic for handling the "Nuovo Ingresso" command
                //_postazioneToMenu.OnNext(Unit.Default);
                await Task.CompletedTask;
            });
        }

        protected override void OnFinalDestruction()
        {
            // Assicuriamoci che la collezione sia nulla per il GC
            //Q = null;
            base.OnFinalDestruction();
        }

        protected override Task OnEsc()
        {
            // Logic for handling the "Esc" command
            _postazioneToMenu.OnNext(Unit.Default);
            return Task.CompletedTask;
        }
    }

    public partial class CassaPostazioneViewModel
    {
        private readonly Subject<Unit> _postazioneToMenu = new();
        public IObservable<Unit> PostazioneToMenu => _postazioneToMenu.AsObservable();

    }
}
