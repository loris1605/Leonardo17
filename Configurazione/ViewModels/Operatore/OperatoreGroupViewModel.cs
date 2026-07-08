using Avalonia.Collections;
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
    public interface IOperatoreGroupViewModel : IRoutableViewModel
    {
        IObservable<Unit> GroupToOperatoreAdd { get; }
        IObservable<int> GroupToOperatoreDel { get; }
        IObservable<int> GroupToOperatoreUpd { get; }
        IObservable<int> GroupToPermessi { get; }
        IObservable<Unit> OperatoreToPostazioni { get; }
        IObservable<Unit> OperatoreToSettori { get; }
        IObservable<Unit> OperatoreToTariffe { get; }
        IObservable<Unit> OperatoreToRientri { get; }
    }

    public partial class OperatoreGroupViewModel : GroupViewModelBase<ConfigurazioneOperatoreMap>, IGroupViewModelBase, IOperatoreGroupViewModel
    {
        public ReactiveCommand<Unit, Unit> PostazioniCommand { get;  }
        public ReactiveCommand<Unit, Unit> SettoriCommand { get;  }
        public ReactiveCommand<Unit, Unit> TariffeCommand { get; }
        public ReactiveCommand<Unit, Unit> PermessiCommand { get;  }
        public ReactiveCommand<Unit, Unit> RientriCommand { get;  }

        private IConfigurazioneOperatoreRepository Q;
        
        protected IConfigurazioneScreen _host;

        public IObservable<bool> CanAction { get; }

        protected override IObservable<bool> CanDel => this.WhenAnyValue(
            x => x.GroupBindingT,
            (item) => item != null && item.CodicePermesso == 0);

        
        protected override IObservable<bool> IsAnythingExecuting =>
        Observable.CombineLatest(
            // 1. Comandi ereditati dalla classe base
            this.WhenAnyObservable(x => x.LoadCommand.IsExecuting).StartWith(false),
            this.WhenAnyObservable(x => x.SaveCommand.IsExecuting).StartWith(false),
            this.WhenAnyObservable(x => x.EscPressedCommand.IsExecuting).StartWith(false),
            // 2. Comandi specifici osservati in modo sicuro direttamente tramite le loro proprietà
            this.WhenAnyObservable(
                x => x.PostazioniCommand.IsExecuting,
                x => x.SettoriCommand.IsExecuting,
                x => x.TariffeCommand.IsExecuting,
                x => x.PermessiCommand.IsExecuting,
                x => x.RientriCommand.IsExecuting
            ).StartWith(false),
            // Se anche uno solo è in esecuzione, restituisce true
            (baseLoad, baseSave, baseEsc, localExec) => baseLoad || baseSave || baseEsc || localExec)
        .DistinctUntilChanged();

        public OperatoreGroupViewModel(IConfigurazioneOperatoreRepository Repository) : base(null)
        {
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
            var canHasSelection = this.WhenAnyValue(x => x.GroupBindingT).Select(item => item != null);

            PostazioniCommand = ReactiveCommand.CreateFromTask(() => GoToGroup(_operatoreToPostazioni));
            SettoriCommand = ReactiveCommand.CreateFromTask(() => GoToGroup(_operatoreToSettori));
            TariffeCommand = ReactiveCommand.CreateFromTask(() => GoToGroup(_operatoreToTariffe));
            RientriCommand = ReactiveCommand.CreateFromTask(() => GoToGroup(_operatoreToRientri));
            PermessiCommand = ReactiveCommand.CreateFromTask(() => OnPermessi(), canHasSelection);

            PostazioniCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Postazioni: {ex.Message}"));
            SettoriCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Settori: {ex.Message}"));
            TariffeCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Tariffe: {ex.Message}"));
            RientriCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Rientri: {ex.Message}"));
            PermessiCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Permessi: {ex.Message}"));
        }

        public void SetHost(IConfigurazioneScreen host) => _host = host;
        
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

        private async Task UpdateCollection(List<ConfigurazioneOperatoreDTO> data, int id)
        {
            var mapped = await Task.Run(() => data.Select(dto => new ConfigurazioneOperatoreMap(dto)).ToList(), Token);
            var view = new DataGridCollectionView(mapped);
            view.GroupDescriptions.Add(new DataGridPathGroupDescription("Titolo"));

            // Sparisce IsLoading = true manuale
            var backup = GroupBindingT;
            GroupBindingT = null;
            GroupedDataSource = view;
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
            _groupToOperatoreAdd.OnNext(Unit.Default);
            await Task.CompletedTask;
        }

        protected async override Task OnDeleting()
        {
            _groupToOperatoreDel.OnNext(GroupBindingT.Id);
            await Task.CompletedTask;
        }

        protected async override Task OnUpdating()
        {
            _groupToOperatoreUpd.OnNext(GroupBindingT.Id);
            await Task.CompletedTask;
        }

        protected async Task OnPermessi()
        {
            _groupToPermessi.OnNext(GroupBindingT.Id);
            await Task.CompletedTask;
        }

        protected override Task OnEsc() => Task.FromResult(Unit.Default);
        
    }

    public partial class OperatoreGroupViewModel
    {
        // 1. Aggiungi questo Subject per notificare l'esterno
        private readonly Subject<Unit> _groupToOperatoreAdd = new();
        public IObservable<Unit> GroupToOperatoreAdd => _groupToOperatoreAdd.AsObservable();

        private readonly Subject<int> _groupToOperatoreDel = new();
        public IObservable<int> GroupToOperatoreDel => _groupToOperatoreDel.AsObservable();

        private readonly Subject<int> _groupToOperatoreUpd = new();
        public IObservable<int> GroupToOperatoreUpd => _groupToOperatoreUpd.AsObservable();

        private readonly Subject<int> _groupToPermessi = new();
        public IObservable<int> GroupToPermessi => _groupToPermessi.AsObservable();

        private readonly Subject<Unit> _operatoreToPostazioni = new();
        public IObservable<Unit> OperatoreToPostazioni => _operatoreToPostazioni.AsObservable();
       
        private readonly Subject<Unit> _operatoreToSettori = new();
        public IObservable<Unit> OperatoreToSettori => _operatoreToSettori.AsObservable();

        private readonly Subject<Unit> _operatoreToTariffe = new();
        public IObservable<Unit> OperatoreToTariffe => _operatoreToTariffe.AsObservable();

        private readonly Subject<Unit> _operatoreToRientri = new();
        public IObservable<Unit> OperatoreToRientri => _operatoreToRientri.AsObservable();


    }
}
