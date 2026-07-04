using Configurazione.Core.DTO;
using Configurazione.Core.Repository;
using Configurazione.ViewModels.Map;
using ReactiveUI;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ViewModels;

namespace Configurazione.ViewModels
{
    public interface ITipoRientroGroupViewModel : IRoutableViewModel
    {
        IObservable<Unit> GroupToTipoRientroAdd { get; }
        IObservable<int> GroupToTipoRientroDel { get; }
        IObservable<int> GroupToTipoRientroUpd { get; }
        IObservable<Unit> TipoRientroToOperatori { get; }
        IObservable<Unit> TipoRientroToPostazioni { get; }
        IObservable<Unit> TipoRientroToSettori { get; }
        IObservable<Unit> TipoRientroToTariffe { get; }
    }

    public partial class TipoRientroGroupViewModel : GroupViewModelBase<ConfigurazioneTipoRientroMap>, 
                                             IGroupViewModelBase, ITipoRientroGroupViewModel
    {
        public ReactiveCommand<Unit, Unit> PostazioniCommand { get; }
        public ReactiveCommand<Unit, Unit> SettoriCommand { get; }
        public ReactiveCommand<Unit, Unit> OperatoriCommand { get; }
        public ReactiveCommand<Unit, Unit> TariffeCommand { get; }

        private ITipoRientroRepository Q;

        // Logica di cancellazione reattiva (IsLoading gestito dalla base)
        protected override IObservable<bool> CanDel => this.WhenAnyValue(
            x => x.GroupBindingT,
            x => x.GroupBindingT.HasPostazione,
            (item, hasPostazione) => item != null && !hasPostazione);

        // Registriamo i nuovi comandi nell'IsLoading globale per automatizzare l'icona di attesa
        protected override IObservable<bool> IsAnythingExecuting =>
        Observable.CombineLatest(
            // 1. Comandi ereditati dalla classe base
            this.WhenAnyObservable(x => x.LoadCommand.IsExecuting).StartWith(false),
            this.WhenAnyObservable(x => x.SaveCommand.IsExecuting).StartWith(false),
            this.WhenAnyObservable(x => x.EscPressedCommand.IsExecuting).StartWith(false),
            // 2. Comandi specifici osservati in modo sicuro direttamente tramite le loro proprietà
            this.WhenAnyObservable(
                x => x.OperatoriCommand.IsExecuting,
                x => x.PostazioniCommand.IsExecuting,
                x => x.SettoriCommand.IsExecuting,
                x => x.TariffeCommand.IsExecuting
            ).StartWith(false),
            // Se anche uno solo è in esecuzione, restituisce true
            (baseLoad, baseSave, baseEsc, localExec) => baseLoad || baseSave || baseEsc || localExec)
        .DistinctUntilChanged();

        public TipoRientroGroupViewModel(ITipoRientroRepository Repository) : base(null)
        {
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));


            var canHasSelection = this.WhenAnyValue(x => x.GroupBindingT).Select(item => item != null);

            OperatoriCommand = ReactiveCommand.CreateFromTask(() => GoToGroup(_tipoRientroToOperatori));
            PostazioniCommand = ReactiveCommand.CreateFromTask(() => GoToGroup(_tipoRientroToPostazioni));
            SettoriCommand = ReactiveCommand.CreateFromTask(() => GoToGroup(_tipoRientroToSettori));
            TariffeCommand = ReactiveCommand.CreateFromTask(() => GoToGroup(_tipoRientroToTariffe));

            OperatoriCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Operatori: {ex.Message}"));
            PostazioniCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Postazioni: {ex.Message}"));
            SettoriCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Settori: {ex.Message}"));
            TariffeCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Tariffe: {ex.Message}"));
        }

        protected override void OnFinalDestruction()
        {
            // Assicuriamoci che la collezione sia nulla per il GC
            Q = null;
            base.OnFinalDestruction();
        }

        protected override async Task OnLoading()
        {
            var data = await Q.Load(0, Token);
            if (data?.Count > 0)
            {
                await UpdateCollection(data, 0);
                GroupBindingT = DataSource.FirstOrDefault();
            }
            else
            {
                DataSource = [];
                GroupedDataSource = null;
            }
        }

        private async Task UpdateCollection(List<ConfigurazioneTipoRientroDTO> data, int id)
        {
            var mapped = await Task.Run(() => data.Select(dto => new ConfigurazioneTipoRientroMap(dto)).ToList(), Token);

            var backup = GroupBindingT;
            GroupBindingT = null;
            DataSource = mapped;
            GroupBindingT = backup;

            IdIndex = id;
            GroupFocus = true;
        }

        public override async Task CaricaDataSource(int id = 0)
        {
            try
            {
                var data = await Q.Load(id, Token);
                await UpdateCollection(data, id);
            }
            catch (OperationCanceledException) { }
        }

        private async Task GoToGroup(Subject<Unit> group)
        {
            _isClosing = true; // Impedisce ulteriori interazioni durante la navigazione
            group.OnNext(Unit.Default);
            group.OnCompleted(); // Completa il flusso per notificare l'esterno
            await Task.CompletedTask;
        }

        protected async override Task OnAdding()
        {
            _groupToTipoRientroAdd.OnNext(Unit.Default);
            await Task.CompletedTask;
        }

        protected async override Task OnDeleting()
        {
            _groupToTipoRientroDel.OnNext(GroupBindingT.Id);
            await Task.CompletedTask;
        }

        protected async override Task OnUpdating()
        {
            _groupToTipoRientroUpd.OnNext(GroupBindingT.Id);
            await Task.CompletedTask;
        }

        protected override async Task OnEsc() => await Task.CompletedTask;

    }

    public partial class TipoRientroGroupViewModel
    {
        // 1. Aggiungi questo Subject per notificare l'esterno
        private readonly Subject<Unit> _groupToTipoRientroAdd = new();
        public IObservable<Unit> GroupToTipoRientroAdd => _groupToTipoRientroAdd.AsObservable();

        private readonly Subject<int> _groupToTipoRientroDel = new();
        public IObservable<int> GroupToTipoRientroDel => _groupToTipoRientroDel.AsObservable();

        private readonly Subject<int> _groupToTipoRientroUpd = new();
        public IObservable<int> GroupToTipoRientroUpd => _groupToTipoRientroUpd.AsObservable();

        private readonly Subject<Unit> _tipoRientroToOperatori = new();
        public IObservable<Unit> TipoRientroToOperatori => _tipoRientroToOperatori.AsObservable();

        private readonly Subject<Unit> _tipoRientroToPostazioni = new();
        public IObservable<Unit> TipoRientroToPostazioni => _tipoRientroToPostazioni.AsObservable();

        private readonly Subject<Unit> _tipoRientroToSettori = new();
        public IObservable<Unit> TipoRientroToSettori => _tipoRientroToSettori.AsObservable();

        private readonly Subject<Unit> _tipoRientroToTariffe = new();
        public IObservable<Unit> TipoRientroToTariffe => _tipoRientroToTariffe.AsObservable();
    }
}
