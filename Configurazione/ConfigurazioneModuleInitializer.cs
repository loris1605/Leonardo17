using Configurazione.Core.Context;
using Configurazione.Core.Repository;
using Configurazione.ViewModels;
using Configurazione.Views;
using Contracts;
using ReactiveUI;
using Splat;

namespace Configurazione
{
    public static class ConfigurazioneModuleInitializer
    {
        public static void Initialize()
        {
            // 1. REGISTRAZIONE COMPONENTI DATI (Interni alla DLL)
            // Registriamo il DbContext specifico del modulo
            Locator.CurrentMutable.Register(() => new OperatoreDbContext(), typeof(IOperatoreDbContext));
            Locator.CurrentMutable.Register(() => new PostazioneDbContext(), typeof(IPostazioneDbContext));
            Locator.CurrentMutable.Register(() => new SettoreDbContext(), typeof(ISettoreDbContext));
            Locator.CurrentMutable.Register(() => new TariffaDbContext(), typeof(ITariffaDbContext));

            // CORRETTO: Spostiamo il GetService dentro l'ambito della Lambda () => ...
            // In questo modo, il DbContext verrà cercato solo quando verrà creato il Repository
            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IOperatoreDbContext>();
                return new ConfigurazioneOperatoreRepository(context);
            }, typeof(IConfigurazioneOperatoreRepository));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IPostazioneDbContext>();
                return new ConfigurazionePostazioneRepository(context);
            }, typeof(IConfigurazionePostazioneRepository));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<ISettoreDbContext>();
                return new ConfigurazioneSettoreRepository(context);
            }, typeof(IConfigurazioneSettoreRepository));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<ITariffaDbContext>();
                return new ConfigurazioneTariffaRepository(context);
            }, typeof(IConfigurazioneTariffaRepository));



            Locator.CurrentMutable.Register(() =>
            {
                var screen = Locator.Current.GetService<IScreen>();
                return new ConfigurazioneViewModel(screen);
            }, typeof(IConfigurazioneViewModel));

            Locator.CurrentMutable.RegisterLazySingleton<IConfigurazioneScreen>(() =>
            {
                // Equivale a sp.GetRequiredService<IConfigurazioneViewModel>()
                var vm = Locator.Current.GetService<IConfigurazioneViewModel>();

                return vm == null
                    ? throw new InvalidOperationException("IConfigurazioneViewModel non è registrato in Splat.")
                    : (IConfigurazioneScreen)vm;
            });

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazioneOperatoreRepository>();
                var host = Locator.Current.GetService<IConfigurazioneScreen>();
                return new OperatoreGroupViewModel(host, context);
            }, typeof(IOperatoreGroupViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazioneOperatoreRepository>();
                var host = Locator.Current.GetService<IConfigurazioneScreen>();
                return new OperatoreAddViewModel(host, context);
            }, typeof(IOperatoreAddViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazionePostazioneRepository>();
                var host = Locator.Current.GetService<IConfigurazioneScreen>();
                return new PostazioneGroupViewModel(host, context);
            }, typeof(IPostazioneGroupViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazioneSettoreRepository>();
                var host = Locator.Current.GetService<IConfigurazioneScreen>();
                return new SettoreGroupViewModel(host, context);
            }, typeof(ISettoreGroupViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazioneTariffaRepository>();
                var host = Locator.Current.GetService<IConfigurazioneScreen>();
                return new TariffaGroupViewModel(host, context);
            }, typeof(ITariffaGroupViewModel));


            // 2. REGISTRAZIONE COMPONENTI UI (Modello B - Usa e Getta)
            // CORRETTO: Spostiamo i resolver dentro la Lambda. 
            // Il ViewModel nascerà solo quando il costruttore verrà invocato dal thread UI di MainWindow
            Locator.CurrentMutable.Register(() => new ConfigurazioneView(), typeof(IViewFor<ConfigurazioneViewModel>));

            Locator.CurrentMutable.Register(() => new OperatoreGroupView(), typeof(IViewFor<OperatoreGroupViewModel>));
            //Locator.CurrentMutable.Register(() => new OperatoreGroupView(), typeof(IViewFor<OperatoreGroupViewModel>));
            Locator.CurrentMutable.Register(() => new PostazioneGroupView(), typeof(IViewFor<PostazioneGroupViewModel>));
            Locator.CurrentMutable.Register(() => new SettoreGroupView(), typeof(IViewFor<SettoreGroupViewModel>));
            Locator.CurrentMutable.Register(() => new TariffaGroupView(), typeof(IViewFor<TariffaGroupViewModel>));


        }
    }
}
