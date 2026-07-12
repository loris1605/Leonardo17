using Cassa.Core.Context;
using Cassa.Core.Repository;
using Cassa.ViewModels;
using Cassa.Views;
using Contracts;
using ReactiveUI;
using Splat;

namespace Cassa
{
    public static class CassaModuleInitializer
    {
        public static void Initialize()
        {

            // 1. REGISTRAZIONE COMPONENTI DATI (Interni alla DLL)
            // Registriamo il DbContext specifico del modulo
            Locator.CurrentMutable.Register(() => new CassaPostazioneDbContext(), typeof(ICassaPostazioneDbContext));
            Locator.CurrentMutable.Register(() => new EntraSocioDbContext(), typeof(IEntraSocioDbContext));
            Locator.CurrentMutable.Register(() => new StrisciateDbContext(), typeof(IStrisciateDbContext));


            // CORRETTO: Spostiamo il GetService dentro l'ambito della Lambda () => ...
            // In questo modo, il DbContext verrà cercato solo quando verrà creato il Repository
            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<ICassaPostazioneDbContext>();
                return new CassaPostazioneRepository(context);
            }, typeof(ICassaPostazioneRepository));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IEntraSocioDbContext>();
                return new EntraSocioRepository(context);
            }, typeof(IEntraSocioRepository));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IEntraSocioDbContext>();
                return new EntraSocioRepository(context);
            }, typeof(IEntraSocioRepository));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IStrisciateDbContext>();
                return new StrisciataRepository(context);
            }, typeof(IStrisciataRepository));




            Locator.CurrentMutable.Register(() => new CassaViewModel(), typeof(ICassaViewModel));
            Locator.CurrentMutable.Register(() => new CassaPostazioneViewModel(), typeof(ICassaPostazioneViewModel));


            // 2. REGISTRAZIONE COMPONENTI UI (Modello B - Usa e Getta)
            // CORRETTO: Spostiamo i resolver dentro la Lambda. 
            // Il ViewModel nascerà solo quando il costruttore verrà invocato dal thread UI di MainWindow
            Locator.CurrentMutable.Register(() => new CassaView(), typeof(IViewFor<CassaViewModel>));

            Locator.CurrentMutable.Register(() => new CassaPostazioneView(), typeof(IViewFor<CassaPostazioneViewModel>));

        }
    }
}
