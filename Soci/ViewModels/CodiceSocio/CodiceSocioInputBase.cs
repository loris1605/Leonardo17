using ReactiveUI;
using Soci.ViewModels;
using Soci.ViewModels.Map;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ViewModels;

namespace Soci.ViewModels
{
    public partial class CodiceSocioInputBase : InputViewModel<SociPersonMap>
    {
        int CodiceSocio => BindingT is null ? 0 : BindingT.CodiceSocio;
        int CodicePerson => BindingT is null ? 0 : BindingT.Id;

        protected string GetNumeroTessera => BindingT?.NumeroTessera?.Trim() ?? string.Empty;
        protected string GetNumeroSocio => BindingT?.NumeroSocio?.Trim() ?? string.Empty;
        protected int GetCodiceSocio => CodiceSocio;
        protected string GetNomeCognome => BindingT is null ? "" : BindingT.Nome + " " + BindingT.Cognome;
        protected int GetCodicePerson => CodicePerson;

        protected ISociScreen _host;

        protected int _idDaModificare;
        protected int _idRitorno;

        public CodiceSocioInputBase() : base()
        {
           
        }

        
        public void SetIdDaModificare(int id)
        {
            _idDaModificare = id;
        }

        public void SetIdRitorno(int id)
        {
            _idRitorno = id;
        }

        protected async override Task OnSaving() { await Task.CompletedTask; }
        protected async override Task OnLoading() { await Task.CompletedTask; }

        protected async override Task OnEsc()
        {
            if (_isClosing) return; // Protezione contro il multi-ESC

            if (_host is not null)
            {
                // Focus sul tasto Esci prima di chiudere
                await SetFocus(EscFocus, 0);
                _isClosing = true; // "Congeliamo" prima di uscire

                _inputEsc.OnNext(Unit.Default); // Notifica l'esterno che ESC è stato premuto

            }
        }

        protected async Task OnBack(int value = 0)
        {
            if (_host is not null)
            {
                
                _isClosing = true;

                _inputBack.OnNext(value); // Notifica l'esterno che Back è stato premuto con il valore specificato
                _inputBack.OnCompleted(); // Completa l'osservabile per evitare memory leak e notificare che non ci saranno più eventi

                await Task.CompletedTask;


            }
        }

    }

    public partial class CodiceSocioInputBase
    {
        public Interaction<Unit, Unit> NumeroSocioFocus { get; } = new();
        public Interaction<Unit, Unit> NumeroTesseraFocus { get; } = new();


        // 1. Aggiungi questo Subject per notificare l'esterno
        private readonly Subject<Unit> _inputEsc = new();
        public IObservable<Unit> InputEsc => _inputEsc.AsObservable();

        private readonly Subject<int> _inputBack = new();
        public IObservable<int> InputBack => _inputBack.AsObservable();
    }
}
