using Cassa.Core.Repository;
using Cassa.ViewModels.Map;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ViewModels;

namespace Cassa.ViewModels
{
    public interface ICassaListaSociViewModel : IRoutableViewModel
    {
        void SetHost(ICassaScreen host);
        void SetPostazioneId(int id);
        void SetPosizione(string numPosizione);

        IObservable<(int postazioneId, string posizione)> ListaSociToPostazione { get; }
    }

    public partial class CassaListaSociViewModel : ViewModelBase, ICassaListaSociViewModel
    {
        private int _postazioneId;
        private ICassaScreen _host;
        private string _posizione;
        private readonly ICassaListaSociRepository Q;

        public CassaListaSociViewModel(ICassaListaSociRepository Repository) : base(null)
        {
            Q = Repository ?? throw new ArgumentNullException(nameof(Repository));
        }

        protected override void OnFinalDestruction()
        {
            // Assicuriamoci che la collezione sia nulla per il GC
            //Q = null;
            base.OnFinalDestruction();
        }

        protected override IObservable<bool> IsAnythingExecuting =>
            new[]
            {
                base.IsAnythingExecuting
                //TesseraCommand?.IsExecuting ?? Observable.Return(false),
                //PosizioneEscCommand?.IsExecuting ?? Observable.Return(false),
                //EntraCommand?.IsExecuting ?? Observable.Return(false),
                //F5Command?.IsExecuting ?? Observable.Return(false)

            }.CombineLatest(values => values.Any(x => x));

        protected override async Task OnLoading()
        {
            Titolo = "POSTAZIONE " + await Q.GetPostazioneName(_postazioneId, Token) +
                                           " - Lista Soci all'interno";
            
            var data = await Q.Load(Token);
            var map = data.Select(x => new CassaSchedaMap(x)).ToList();
            DataSource = map;

            await Task.CompletedTask;
        }

        protected async override Task OnEsc()
        {
            _isClosing = true; // Imposta il flag per indicare che stiamo chiudendo la pagina
            _listaSociToPostazione.OnNext((_postazioneId, _posizione)); // Notifica l'esterno
            _listaSociToPostazione.OnCompleted(); // Completa l'osservabile per evitare memory leak    
            await Task.CompletedTask;

        }

        // Chiamata dalla View in caso di doppio click su una riga: notifica l'esterno per tornare alla postazione
        public void ReturnToPostazione(string posizione)
        {
            _listaSociToPostazione.OnNext((_postazioneId, posizione));
            _listaSociToPostazione.OnCompleted();
        }
    }

    public partial class CassaListaSociViewModel
    {

        public void SetHost(ICassaScreen host)
        {
            _host = host;
        }
        public void SetPostazioneId(int id)
        {
            _postazioneId = id;
        }
        public void SetPosizione(string numPosizione)
        {
            _posizione = numPosizione;
        }

        private string _titolo = string.Empty;
        public string Titolo
        {
            get => _titolo;
            set => this.RaiseAndSetIfChanged(ref _titolo, value);
        }

        private CassaSchedaMap bindingt = new();
        public CassaSchedaMap BindingT
        {
            get => bindingt;
            set => this.RaiseAndSetIfChanged(ref bindingt, value);

        }

        private List<CassaSchedaMap> _datasource = [];
        public List<CassaSchedaMap> DataSource
        {
            get => _datasource;
            set => this.RaiseAndSetIfChanged(ref _datasource, value);
        }

        private readonly Subject<(int postazioneId, string posizione)> _listaSociToPostazione = new();
        public IObservable<(int postazioneId, string posizione)> ListaSociToPostazione => _listaSociToPostazione.AsObservable();
    }
}
