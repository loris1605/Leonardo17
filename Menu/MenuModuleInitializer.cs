using Contracts;
using DTO.Repository;
using Menu.Core.Context;
using Menu.ViewModels;
using Menu.Views;
using ReactiveUI;
using Splat;

namespace Menu
{
    public static class MenuModuleInitializer
    {
        public static void Initialize()
        {
            // 1. REGISTRAZIONE COMPONENTI DATI (Interni alla DLL)
            // Registriamo il DbContext specifico del modulo
            Locator.CurrentMutable.Register(() => new MenuDbContext(), typeof(IMenuDbContext));

            // Registrazione repository con factory, coerente con gli altri moduli (lazy)
            Locator.CurrentMutable.Register(() => new MenuRepository(), typeof(IMenuRepository));


            // 2. REGISTRAZIONE COMPONENTI UI (Modello B - Usa e Getta)
            Locator.CurrentMutable.Register(() =>
            {
                var repository = Locator.Current.GetService<IMenuRepository>();
                return new MenuViewModel(repository);
            }, typeof(IMenuViewModel));

            // Registriamo la View associata all'interfaccia e alla classe concreta per il Router
            Locator.CurrentMutable.Register(() => new MenuView(), typeof(IViewFor<MenuViewModel>));

            System.Diagnostics.Debug.WriteLine("***** [DLL-INIT] Menu Registrazioni Splat completate in modalità Lazy *****");
        }
    }
}
