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
