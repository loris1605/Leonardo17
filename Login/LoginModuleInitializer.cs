using Contracts;
using Login.Core.Context;
using Login.Core.Repository;
using Login.ViewModels;
using Login.Views;
using ReactiveUI;
using Splat;

namespace Login
{
    public static class LoginModuleInitializer
    {
        public static void Initialize()
        {
            // 1. REGISTRAZIONE COMPONENTI DATI (Interni alla DLL)
            Locator.CurrentMutable.Register(() => new LoginDbContext(), typeof(ILoginDbContext));

            // Registrazione del repository (lazy) - usa il costruttore esistente senza parametri
            Locator.CurrentMutable.Register(() => new LoginRepository(), typeof(ILoginRepository));
            
            // 2. REGISTRAZIONE COMPONENTI UI (Modello B - Usa e Getta)
            Locator.CurrentMutable.Register(() =>
            {
                var repository = Locator.Current.GetService<ILoginRepository>();
                return new LoginViewModel(repository);
            }, typeof(ILoginViewModel));

            Locator.CurrentMutable.Register(() => new LoginView(), typeof(IViewFor<LoginViewModel>));

            System.Diagnostics.Debug.WriteLine("***** [DLL-INIT] Login Registrazioni Splat completate in modalità Lazy *****");
        }
    }
}
