using ReactiveUI;
using Servizi.Core.DTO;
using Servizi.Core.Repository;
using Servizi.ViewModels.Map;
using System.Reactive;
using System.Reactive.Subjects;
using ViewModels;

namespace Servizi.ViewModels
{
    public interface IAbbonamentoGroupViewModel : IRoutableViewModel
    {
        IObservable<Unit> GroupToAbbonamentoAdd { get; }
        IObservable<int> GroupToAbbonamentoDel { get; }
        IObservable<int> GroupToAbbonamentoUpd { get; }
    }


    public partial class AbbonamentoGroupViewModel : GroupViewModelBase<ServiziTipoAbbonamentoMap>, IGroupViewModelBase, IAbbonamentoGroupViewModel
    {
        private IServiziAbbonamentoRepository Q;

        protected IServiziScreen _host;

        public IObservable<bool> CanAction { get; }

        protected override IObservable<bool> CanDel => this.WhenAnyValue(x => x.GroupBindingT != null);



        public AbbonamentoGroupViewModel(IServiziAbbonamentoRepository repository) : base(null)
        {
            Q = repository ?? throw new ArgumentNullException(nameof(repository));
            CanAction = this.WhenAnyValue(x => x.GroupBindingT != null);

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

        private async Task UpdateCollection(List<ServiziTipoAbbonamentoDTO> data, int id)
        {
            var mapped = await Task.Run(() => data.Select(dto => new ServiziTipoAbbonamentoMap(dto)).ToList(), Token);

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
            _groupToAbbonamentoAdd.OnNext(Unit.Default);
            await Task.CompletedTask;
        }

        protected async override Task OnDeleting()
        {
            _groupToAbbonamentoDel.OnNext(GroupBindingT.Id);
            await Task.CompletedTask;
        }

        protected async override Task OnUpdating()
        {
            _groupToAbbonamentoUpd.OnNext(GroupBindingT.Id);
            await Task.CompletedTask;
        }

        protected override async Task OnEsc() => await Task.CompletedTask;

    }

    public partial class AbbonamentoGroupViewModel
    {
        private Subject<Unit> _groupToAbbonamentoAdd = new();
        public IObservable<Unit> GroupToAbbonamentoAdd => _groupToAbbonamentoAdd;
        private Subject<int> _groupToAbbonamentoDel = new();
        public IObservable<int> GroupToAbbonamentoDel => _groupToAbbonamentoDel;
        private Subject<int> _groupToAbbonamentoUpd = new();
        public IObservable<int> GroupToAbbonamentoUpd => _groupToAbbonamentoUpd;
    }
}
