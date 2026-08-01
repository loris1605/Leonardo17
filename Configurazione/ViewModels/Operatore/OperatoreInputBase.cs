using Configurazione.ViewModels.Map;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ViewModels;

namespace Configurazione.ViewModels
{
    public partial class OperatoreInputBase : InputViewModel<ConfigurazioneOperatoreMap>
    {
        protected int CodiceOperatore => BindingT?.Id ?? 0;

        public int GetCodiceOperatore => CodiceOperatore;

        public bool IsNicknameEmpty => string.IsNullOrWhiteSpace(BindingT?.NomeOperatore);
        public bool IsPasswordEmpty => string.IsNullOrWhiteSpace(BindingT?.Password);

        // Protezione per evitare NullReferenceException o errori di lunghezza su stringhe nulle
        public bool CheckLess2Nickname => (BindingT?.NomeOperatore?.Length ?? 0) < 2;
        public bool CheckLess2Password => (BindingT?.Password?.Length ?? 0) < 2;

        public Interaction<Unit, Unit> NomeFocus { get; } = new();
        public Interaction<Unit, Unit> PasswordFocus { get; } = new();

        protected IConfigurazioneScreen _host;

        protected int _idDaModificare;
        protected int _idRitorno;

        public OperatoreInputBase() : base()
        {

            this.WhenActivated(d =>
            {

                this.WhenAnyValue(x => x.BindingT.Abilitato)
                    .Select(val => val ? "Si" : "No") // Trasforma il bool in stringa
                    .Subscribe(text => AbilitatoText = text) // Assegna il risultato
                    .DisposeWith(d);
            });
        }

        public void SetIdDaModificare(int id)
        {
            _idDaModificare = id;
        }

        public void SetIdRitorno(int id)
        {
            _idRitorno = id;
        }

        protected async Task<bool> ValidaDati()
        {
            if (IsNicknameEmpty)
            {
                InfoLabel = "Inserire il nome dell'operatore";
                await SetFocus(NomeFocus);
                return false;
            }

            if (IsPasswordEmpty)
            {
                InfoLabel = "Inserire la password di accesso";
                await SetFocus(PasswordFocus);
                return false;
            }

            if (CheckLess2Nickname)
            {
                InfoLabel = "Formato nome non valido (min. 2 caratteri)";
                await SetFocus(NomeFocus);
                return false;
            }

            if (CheckLess2Password)
            {
                InfoLabel = "Formato password non valido (min. 2 caratteri)";
                await SetFocus(PasswordFocus);
                return false;
            }


            InfoLabel = ""; // Pulisce eventuali errori precedenti
            return true;
        }

        protected async override Task OnEsc()
        {
            if (_isClosing) return; // Protezione contro il multi-ESC

            await SetFocus(EscFocus, 0);
            _isClosing = true; // "Congeliamo" prima di uscire

            _inputEsc.OnNext(Unit.Default); // Notifica l'esterno che ESC è stato premuto
            _inputEsc.OnCompleted(); // Completa l'osservabile per evitare memory leak e notificare che non ci saranno più eventi

        }

        protected async Task OnBack(int value = 0)
        {
            _isClosing = true;

            _inputBack.OnNext(value); // Notifica l'esterno che Back è stato premuto con il valore specificato
            _inputBack.OnCompleted();
            await Task.CompletedTask;

        }
    }

    public partial class OperatoreInputBase
    {
  
        private string abilitatotext = string.Empty;
        public string AbilitatoText
        {
            get => abilitatotext;
            set => this.RaiseAndSetIfChanged(ref abilitatotext, value);
        }

        private bool nomeoperatoreenabled = true;
        public bool NomeOperatoreEnabled
        {
            get => nomeoperatoreenabled;
            set => this.RaiseAndSetIfChanged(ref nomeoperatoreenabled, value);
        }

        // 1. Aggiungi questo Subject per notificare l'esterno
        private readonly Subject<Unit> _inputEsc = new();
        public IObservable<Unit> InputEsc => _inputEsc.AsObservable();

        private readonly Subject<int> _inputBack = new();
        public IObservable<int> InputBack => _inputBack.AsObservable();


    }
}
