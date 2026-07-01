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
    public interface ISettoreGroupViewModel : IRoutableViewModel
    {
        IObservable<Unit> GroupToSettoreAdd { get; }
        IObservable<int> GroupToSettoreDel { get; }
        IObservable<int> GroupToSettoreUpd { get; }
        IObservable<int> GroupToListino { get; }

        IObservable<Unit> SettoriToOperatori { get; }
        IObservable<Unit> SettoriToPostazioni { get; }
        IObservable<Unit> SettoriToTariffe { get; }
    }

    public partial class SettoreGroupViewModel : GroupViewModelBase<ConfigurazioneSettoreMap>, 
                                                IGroupViewModelBase, ISettoreGroupViewModel
    {
        public ReactiveCommand<Unit, Unit> OperatoriCommand { get; }
        public ReactiveCommand<Unit, Unit> PostazioniCommand { get;  }
        public ReactiveCommand<Unit, Unit> TariffeCommand { get;  }
        public ReactiveCommand<Unit, Unit> ListiniCommand { get;  }
        public ReactiveCommand<Unit, Unit> RepartiCommand { get;  }

        private IConfigurazioneSettoreRepository Q;
        
        protected IConfigurazioneScreen _host;

        //fa il merge con la IObservable base
        protected override IObservable<bool> CanDel => this.WhenAnyValue(
            x => x.GroupBindingT,
            x => x.GroupBindingT.CodiceListino, // Osserva esplicitamente la proprietà interna
            x => x.GroupBindingT.HasReparto,
            (item, codiceSocio, hasReparto) => item != null && codiceSocio == 0 && !hasReparto
        );

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
                x => x.TariffeCommand.IsExecuting,
                x => x.ListiniCommand.IsExecuting,
                x => x.RepartiCommand.IsExecuting
            ).StartWith(false),
            // Se anche uno solo è in esecuzione, restituisce true
            (baseLoad, baseSave, baseEsc, localExec) => baseLoad || baseSave || baseEsc || localExec)
        .DistinctUntilChanged();

        public SettoreGroupViewModel(IConfigurazioneSettoreRepository Repository) : base(null)
        {
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
            

            var canHasSelection = this.WhenAnyValue(x => x.GroupBindingT).Select(item => item != null);


            // Navigazioni Semplici (NavigateAndReset)
            OperatoriCommand = ReactiveCommand.CreateFromTask(() => GoToGroup(_settoriToOperatori));
            PostazioniCommand = ReactiveCommand.CreateFromTask(() => GoToGroup(_settoriToPostazioni));
            TariffeCommand = ReactiveCommand.CreateFromTask(() => GoToGroup(_settoriToTariffe));
            ListiniCommand = ReactiveCommand.CreateFromTask(() => OnListini(), canHasSelection);

            OperatoriCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Operatori: {ex.Message}"));
            PostazioniCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Postazioni: {ex.Message}"));
            TariffeCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Tariffe: {ex.Message}"));
            ListiniCommand.ThrownExceptions.Subscribe(ex => Debug.WriteLine($"Errore Selezione Listini: {ex.Message}"));
        }

        //public void SetHost(IConfigurazioneScreen host) => _host = host;

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

        private async Task UpdateCollection(List<ConfigurazioneSettoreDTO> data, int id)
        {
            var mapped = await Task.Run(() => data.Select(dto => new ConfigurazioneSettoreMap(dto)).ToList(), Token);
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

        protected async override Task OnAdding()
        {
            _groupToSettoreAdd.OnNext(Unit.Default);
            await Task.CompletedTask;
        }

        protected async override Task OnDeleting()
        {
            _groupToSettoreDel.OnNext(GroupBindingT.Id);
            await Task.CompletedTask;
        }

        protected async override Task OnUpdating()
        {
            _groupToSettoreUpd.OnNext(GroupBindingT.Id);
            await Task.CompletedTask;
        }

        protected async Task OnListini()
        {
            _groupToListino.OnNext(GroupBindingT.Id);
            await Task.CompletedTask;
        }

        protected override async Task OnEsc() => await Task.CompletedTask;
        

    }

    public partial class SettoreGroupViewModel
    {
        // 1. Aggiungi questo Subject per notificare l'esterno
        private readonly Subject<Unit> _groupToSettoreAdd = new();
        public IObservable<Unit> GroupToSettoreAdd => _groupToSettoreAdd.AsObservable();

        private readonly Subject<int> _groupToSettoreDel = new();
        public IObservable<int> GroupToSettoreDel => _groupToSettoreDel.AsObservable(); 

        private readonly Subject<int> _groupToSettoreUpd = new();
        public IObservable<int> GroupToSettoreUpd => _groupToSettoreUpd.AsObservable();

        private readonly Subject<int> _groupToListino = new();
        public IObservable<int> GroupToListino => _groupToListino.AsObservable();

        private readonly Subject<Unit> _settoriToOperatori = new();
        public IObservable<Unit> SettoriToOperatori => _settoriToOperatori.AsObservable();

        private readonly Subject<Unit> _settoriToPostazioni = new();
        public IObservable<Unit> SettoriToPostazioni => _settoriToPostazioni.AsObservable();

        private readonly Subject<Unit> _settoriToTariffe = new();
        public IObservable<Unit> SettoriToTariffe => _settoriToTariffe.AsObservable();
    }
}
