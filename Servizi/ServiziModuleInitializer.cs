using Contracts;
using ReactiveUI;
using Servizi.Core.Context;
using Servizi.Core.Repository;
using Servizi.ViewModels;
using Servizi.Views;
using Splat;

namespace Servizi
{
    public static class ServiziModuleInitializer
    {
        public static void Initialize()
        {
            // 1. REGISTRAZIONE COMPONENTI DATI (Interni alla DLL)
            // Registriamo il DbContext specifico del modulo
            Locator.CurrentMutable.Register(() => new ServiziAbbonamentoDbContext(), typeof(IServiziAbbonamentoDbContext));



            // CORRETTO: Spostiamo il GetService dentro l'ambito della Lambda () => ...
            // In questo modo, il DbContext verrà cercato solo quando verrà creato il Repository
            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IServiziAbbonamentoDbContext>();
                return new ServiziAbbonamentoRepository(context);
            }, typeof(IServiziAbbonamentoRepository));



            Locator.CurrentMutable.Register(() => new ServiziViewModel(), typeof(IServiziViewModel));



            Locator.CurrentMutable.Register(() => new ServiziView(), typeof(IViewFor<ServiziViewModel>));


            // Initialization code for the Servizi module
            Console.WriteLine("Servizi module initialized.");




        }
    }
}
