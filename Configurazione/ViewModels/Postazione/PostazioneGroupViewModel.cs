using Avalonia.Collections;
using Configurazione.Core.DTO;
using Configurazione.Core.Repository;
using Configurazione.ViewModels;
using Configurazione.ViewModels.Map;
using ReactiveUI;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ViewModels;

namespace Configurazione.ViewModels
{
    public interface IPostazioneGroupViewModel : IRoutableViewModel
    {
        IObservable<Unit> GroupToPostazioneAdd { get; }
        IObservable<int> GroupToPostazioneDel { get; }
        IObservable<int> GroupToPostazioneUpd { get; }
        IObservable<Unit> PostazioniToOperatori { get; }
        IObservable<Unit> PostazioniToSettori { get; }
        IObservable<Unit> PostazioniToTariffe { get; }
    }

    public partial class PostazioneGroupViewModel : GroupViewModelBase<ConfigurazionePostazioneMap>, 
                                            IGroupViewModelBase, IPostazioneGroupViewModel
    {
        public ReactiveCommand<Unit, Unit> OperatoriCommand { get; }
        public ReactiveCommand<Unit, Unit> SettoriCommand { get;  }
        public ReactiveCommand<Unit, Unit> TariffeCommand { get;  }
        public ReactiveCommand<Unit, Unit> RepartiCommand { get;  }

        private IConfigurazionePostazioneRepository Q;
        
        protected IConfigurazioneScreen _host;

        public IObservable<bool> CanAction { get; }

        //fa il merge con la IObservable base
        protected override IObservable<bool> CanDel => this.WhenAnyValue(
            x => x.GroupBindingT,
            x => x.GroupBindingT.CodiceReparto,
            x => x.GroupBindingT.HasPermesso,
            (item, codiceSocio, hasP) => item != null && codiceSocio == 0 && !hasP
        );

        protected override IObservable<bool> IsAnythingExecuting =>
        Observable.CombineLatest(
            // 1. Comandi ereditati dalla classe base
            this.WhenAnyObservable(x => x.LoadCommand.IsExecuting).StartWith(false),
            this.WhenAnyObservable(x => x.SaveCommand.IsExecuting).StartWith(false),
            this.WhenAnyObservable(x => x.EscPressedCommand.IsExecuting).StartWith(false),
            // 2. Comandi specifici osservati in modo sicuro direttamente tramite le loro proprietà
            this.WhenAnyObservable(
                x => x.OperatoriCommand.IsExecuting,
                x => x.SettoriCommand.IsExecuting,
                x => x.TariffeCommand.IsExecuting,
                x => x.RepartiCommand.IsExecuting
            ).StartWith(false),
            // Se anche uno solo è in esecuzione, restituisce true
            (baseLoad, baseSave, baseEsc, localExec) => baseLoad || baseSave || baseEsc || localExec)
        .DistinctUntilChanged();



        public PostazioneGroupViewModel(IConfigurazioneScreen host,
                                        IConfigurazionePostazioneRepository Repository) : base(null)
        {
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
            _host = host ?? throw new ArgumentNullException(nameof(host));

            var canHasSelection = this.WhenAnyValue(x => x.GroupBindingT).Select(item => item != null);

            OperatoriCommand = ReactiveCommand.CreateFromTask(() => GoToGroup(_postazioniToOperatori));
            SettoriCommand = ReactiveCommand.CreateFromTask(() => GoToGroup(_postazioniToSettori));
            TariffeCommand = ReactiveCommand.CreateFromTask(() => GoToGroup(_postazioniToTariffe));

            OperatoriCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Operatori: {ex.Message}"));
            SettoriCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Settori: {ex.Message}"));
            TariffeCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Tariffe: {ex.Message}"));
        }

              
        protected override void OnFinalDestruction()
        {
            Q = null;
            _host = null;
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

        private async Task UpdateCollection(List<ConfigurazionePostazioneDTO> data, int id)
        {
            var mapped = await Task.Run(() => data.Select(dto => new ConfigurazionePostazioneMap(dto)).ToList(), Token);
            var view = new DataGridCollectionView(mapped);
            view.GroupDescriptions.Add(new DataGridPathGroupDescription("Titolo"));

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

        protected async override Task OnAdding() => await Task.CompletedTask;

        protected async override Task OnDeleting() =>
                                    await Task.CompletedTask;

        protected async override Task OnUpdating() =>
                                    await Task.CompletedTask;

        protected override Task OnEsc() => Task.FromResult(Unit.Default);
    }

    public partial class PostazioneGroupViewModel
    {
        // 1. Aggiungi questo Subject per notificare l'esterno
        private readonly Subject<Unit> _groupToPostazioneAdd = new();
        public IObservable<Unit> GroupToPostazioneAdd => _groupToPostazioneAdd.AsObservable();

        private readonly Subject<int> _groupToPostazioneDel = new();
        public IObservable<int> GroupToPostazioneDel => _groupToPostazioneDel.AsObservable();

        private readonly Subject<int> _groupToPostazioneUpd = new();
        public IObservable<int> GroupToPostazioneUpd => _groupToPostazioneUpd.AsObservable();

        private readonly Subject<Unit> _postazioniToOperatori = new();
        public IObservable<Unit> PostazioniToOperatori => _postazioniToOperatori.AsObservable();

        private readonly Subject<Unit> _postazioniToSettori = new();
        public IObservable<Unit> PostazioniToSettori => _postazioniToSettori.AsObservable();

        private readonly Subject<Unit> _postazioniToTariffe = new();
        public IObservable<Unit> PostazioniToTariffe => _postazioniToTariffe.AsObservable();
    }
}
