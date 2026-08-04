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
            // Registriamo sia l'interfaccia che il tipo concreto per facilitare la risoluzione
            Locator.CurrentMutable.Register(() => new LoginDbContext(), typeof(ILoginDbContext));
            Locator.CurrentMutable.Register(() => new LoginDbContext(), typeof(LoginDbContext));

            // Registrazione del repository (lazy) - passa la factory del DbContext
            Locator.CurrentMutable.Register(() =>
            {
                return new LoginRepository(() => new LoginDbContext());
            }, typeof(ILoginRepository));

            // 2. REGISTRAZIONE COMPONENTI UI (Modello B - Usa e Getta)
            Locator.CurrentMutable.Register(() =>
            {
                // Risolviamo in modo null-safe e falliamo esplicitamente se manca la registrazione
                var repository = Locator.Current.GetService<ILoginRepository>()
                    ?? throw new InvalidOperationException("ILoginRepository non registrato in Splat (LoginModuleInitializer).");
                return new LoginViewModel(repository);
            }, typeof(ILoginViewModel));

            Locator.CurrentMutable.Register(() => new LoginView(), typeof(IViewFor<LoginViewModel>));

            System.Diagnostics.Debug.WriteLine("***** [DLL-INIT] Login Registrazioni Splat completate in modalità Lazy *****");
        }
    }
}
