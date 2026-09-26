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

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IServiziAbbonamentoRepository>();
                return new AbbonamentoGroupViewModel(context);
            }, typeof(IAbbonamentoGroupViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IServiziAbbonamentoRepository>();
                return new AbbonamentoAddViewModel(context);
            }, typeof(IAbbonamentoAddViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IServiziAbbonamentoRepository>();
                return new AbbonamentoDelViewModel(context);
            }, typeof(IAbbonamentoDelViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IServiziAbbonamentoRepository>();
                return new AbbonamentoUpdViewModel(context);
            }, typeof(IAbbonamentoUpdViewModel));


            Locator.CurrentMutable.Register(() => new ServiziView(), typeof(IViewFor<ServiziViewModel>));

            Locator.CurrentMutable.Register(() => new AbbonamentoGroupView(), typeof(IViewFor<AbbonamentoGroupViewModel>));


            // Initialization code for the Servizi module
            Console.WriteLine("Servizi module initialized.");




        }
    }
}
