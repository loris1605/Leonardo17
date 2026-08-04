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
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            Id = dto.Id;
            NomeOperatore = dto.NomeOperatore;
            Password = dto.Password;
        }

        public LoginDTO ToDto()
        {
            return new LoginDTO
            {
                Id = Id,
                NomeOperatore = NomeOperatore ?? string.Empty,
                Password = Password ?? string.Empty
            };
        }

        private string _nomeoperatore = string.Empty;
        public string NomeOperatore
        {
            get => _nomeoperatore;
            set => this.RaiseAndSetIfChanged(ref _nomeoperatore, value ?? string.Empty);
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value ?? string.Empty);
        }

        public override string Titolo => $"Login: {NomeOperatore}";


    }
}
