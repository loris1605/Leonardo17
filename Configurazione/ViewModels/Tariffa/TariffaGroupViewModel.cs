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
    public interface ITariffaGroupViewModel : IRoutableViewModel
    {
        IObservable<Unit> GroupToTariffaAdd { get; }
        IObservable<int> GroupToTariffaDel { get; }
        IObservable<int> GroupToTariffaUpd { get; }
        IObservable<Unit> TariffaToOperatori { get; }
        IObservable<Unit> TariffaToPostazioni { get; }
        IObservable<Unit> TariffaToSettori { get; }
    }

    public partial class TariffaGroupViewModel : GroupViewModelBase<ConfigurazioneTariffaMap>, IGroupViewModelBase, ITariffaGroupViewModel
    {

        public ReactiveCommand<Unit, Unit> PostazioniCommand { get;}
        public ReactiveCommand<Unit, Unit> SettoriCommand { get;  }
        public ReactiveCommand<Unit, Unit> OperatoriCommand { get;  }
        public ReactiveCommand<Unit, Unit> ListiniCommand { get;  }

        private IConfigurazioneTariffaRepository Q;

        protected IConfigurazioneScreen _host;

        // Logica di cancellazione reattiva (IsLoading gestito dalla base)
        protected override IObservable<bool> CanDel => this.WhenAnyValue(
            x => x.GroupBindingT,
            x => x.GroupBindingT.HasListino,
            (item, hasListino) => item != null && !hasListino);

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
                x => x.ListiniCommand.IsExecuting
            ).StartWith(false),
            // Se anche uno solo è in esecuzione, restituisce true
            (baseLoad, baseSave, baseEsc, localExec) => baseLoad || baseSave || baseEsc || localExec)
        .DistinctUntilChanged();



        public TariffaGroupViewModel(IConfigurazioneTariffaRepository Repository) : base(null)
        {
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
            

            var canHasSelection = this.WhenAnyValue(x => x.GroupBindingT).Select(item => item != null);

            OperatoriCommand = ReactiveCommand.CreateFromTask(() => GoToGroup(_tariffaToOperatori));
            PostazioniCommand = ReactiveCommand.CreateFromTask(() => GoToGroup(_tariffaToPostazioni));
            SettoriCommand = ReactiveCommand.CreateFromTask(() => GoToGroup(_tariffaToSettori));

            OperatoriCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Operatori: {ex.Message}"));
            PostazioniCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Postazioni: {ex.Message}"));
            SettoriCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Settori: {ex.Message}"));

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

        private async Task UpdateCollection(List<ConfigurazioneTariffaDTO> data, int id)
        {
            var mapped = await Task.Run(() => data.Select(dto => new ConfigurazioneTariffaMap(dto)).ToList(), Token);

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
            _groupToTariffaAdd.OnNext(Unit.Default);
            await Task.CompletedTask;
        }

        protected async override Task OnDeleting()
        {
            _groupToTariffaDel.OnNext(GroupBindingT.Id);
            await Task.CompletedTask;
        }

        protected async override Task OnUpdating()
        {
            _groupToTariffaUpd.OnNext(GroupBindingT.Id);
            await Task.CompletedTask;
        }

        protected override async Task OnEsc() => await Task.CompletedTask;

    }

    public partial class TariffaGroupViewModel
    {
        // 1. Aggiungi questo Subject per notificare l'esterno
        private readonly Subject<Unit> _groupToTariffaAdd = new();
        public IObservable<Unit> GroupToTariffaAdd => _groupToTariffaAdd.AsObservable();

        private readonly Subject<int> _groupToTariffaDel = new();
        public IObservable<int> GroupToTariffaDel => _groupToTariffaDel.AsObservable(); 

        private readonly Subject<int> _groupToTariffaUpd = new();
        public IObservable<int> GroupToTariffaUpd => _groupToTariffaUpd.AsObservable();

        private readonly Subject<Unit> _tariffaToOperatori = new();
        public IObservable<Unit> TariffaToOperatori => _tariffaToOperatori.AsObservable();

        private readonly Subject<Unit> _tariffaToPostazioni = new();
        public IObservable<Unit> TariffaToPostazioni => _tariffaToPostazioni.AsObservable();

        private readonly Subject<Unit> _tariffaToSettori = new();
        public IObservable<Unit> TariffaToSettori => _tariffaToSettori.AsObservable();
    }
}
