using Login.Core.DTO;
using ReactiveUI;
using ViewModelServices.Core.Map;

namespace Login.ViewModels.Map
{
    public class LoginMap : BindableMap
    {
        public LoginMap() { }

        public LoginMap(LoginDTO dto)
        {
            Id = dto.Id;
            NomeOperatore = dto.NomeOperatore;
            Password = dto.Password;
        }

        public LoginDTO ToDto()
        {
            return new LoginDTO
            {
                Id = Id,
                NomeOperatore = NomeOperatore,
                Password = Password
            };
        }
        
        private string _nomeoperatore;
        public string NomeOperatore
        {
            get => _nomeoperatore;
            set => this.RaiseAndSetIfChanged(ref _nomeoperatore, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        public override string Titolo => $"Login: {NomeOperatore}";

        
    }
}
