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
            Locator.CurrentMutable.Register(() => new PermessoDbContext(), typeof(IPermessoDbContext));
            Locator.CurrentMutable.Register(() => new RepartoDbContext(), typeof(IRepartoDbContext));
            Locator.CurrentMutable.Register(() => new ListinoDbContext(), typeof(IListinoDbContext));

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
                var context = Locator.Current.GetService<IPermessoDbContext>();
                return new PermessoRepository(context);
            }, typeof(IPermessoRepository));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IRepartoDbContext>();
                return new ConfigurazioneRepartoRepository(context);
            }, typeof(IConfigurazioneRepartoRepository));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IListinoDbContext>();
                return new ConfigurazioneListinoRepository(context);
            }, typeof(IConfigurazioneListinoRepository));


            Locator.CurrentMutable.Register(() => new ConfigurazioneViewModel(), typeof(IConfigurazioneViewModel));

            
            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazioneOperatoreRepository>();
                return new OperatoreGroupViewModel(context);
            }, typeof(IOperatoreGroupViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazioneOperatoreRepository>();
                return new OperatoreAddViewModel(context);
            }, typeof(IOperatoreAddViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazioneOperatoreRepository>();
                return new OperatoreDelViewModel(context);
            }, typeof(IOperatoreDelViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazioneOperatoreRepository>();
                return new OperatoreUpdViewModel(context);
            }, typeof(IOperatoreUpdViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context1 = Locator.Current.GetService<IPermessoRepository>();
                var context2 = Locator.Current.GetService<IConfigurazioneOperatoreRepository>();
                return new PermessiViewModel(context1, context2);
            }, typeof(IPermessoViewModel));



            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazionePostazioneRepository>();
                return new PostazioneGroupViewModel(context);
            }, typeof(IPostazioneGroupViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazionePostazioneRepository>();
                return new PostazioneAddViewModel(context);
            }, typeof(IPostazioneAddViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazionePostazioneRepository>();
                return new PostazioneDelViewModel(context);
            }, typeof(IPostazioneDelViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazionePostazioneRepository>();
                return new PostazioneUpdViewModel(context);
            }, typeof(IPostazioneUpdViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context1 = Locator.Current.GetService<IConfigurazionePostazioneRepository>();
                var context2 = Locator.Current.GetService<IConfigurazioneRepartoRepository>();
                return new RepartiViewModel(context1, context2);
            }, typeof(IRepartoViewModel));



            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazioneSettoreRepository>();
                return new SettoreGroupViewModel(context);
            }, typeof(ISettoreGroupViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazioneSettoreRepository>();
                return new SettoreAddViewModel(context);
            }, typeof(ISettoreAddViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazioneSettoreRepository>();
                return new SettoreDelViewModel(context);
            }, typeof(ISettoreDelViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazioneSettoreRepository>();
                return new SettoreUpdViewModel(context);
            }, typeof(ISettoreUpdViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context1 = Locator.Current.GetService<IConfigurazioneSettoreRepository>();
                var context2 = Locator.Current.GetService<IConfigurazioneListinoRepository>();
                return new ListinoViewModel(context1, context2);
            }, typeof(IListinoViewModel));



            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazioneTariffaRepository>();
                return new TariffaGroupViewModel(context);
            }, typeof(ITariffaGroupViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazioneTariffaRepository>();
                return new TariffaAddViewModel(context);
            }, typeof(ITariffaAddViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazioneTariffaRepository>();
                return new TariffaDelViewModel(context);
            }, typeof(ITariffaDelViewModel));

            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IConfigurazioneTariffaRepository>();
                return new TariffaUpdViewModel(context);
            }, typeof(ITariffaUpdViewModel));




            // 2. REGISTRAZIONE COMPONENTI UI (Modello B - Usa e Getta)
            // CORRETTO: Spostiamo i resolver dentro la Lambda. 
            // Il ViewModel nascerà solo quando il costruttore verrà invocato dal thread UI di MainWindow
            Locator.CurrentMutable.Register(() => new ConfigurazioneView(), typeof(IViewFor<ConfigurazioneViewModel>));

            Locator.CurrentMutable.Register(() => new OperatoreGroupView(), typeof(IViewFor<OperatoreGroupViewModel>));
            Locator.CurrentMutable.Register(() => new OperatoreInputView(), typeof(IViewFor<OperatoreAddViewModel>));
            Locator.CurrentMutable.Register(() => new OperatoreInputView(), typeof(IViewFor<OperatoreDelViewModel>));
            Locator.CurrentMutable.Register(() => new OperatoreInputView(), typeof(IViewFor<OperatoreUpdViewModel>));

            Locator.CurrentMutable.Register(() => new PermessiView(), typeof(IViewFor<PermessiViewModel>));


            Locator.CurrentMutable.Register(() => new PostazioneGroupView(), typeof(IViewFor<PostazioneGroupViewModel>));
            Locator.CurrentMutable.Register(() => new PostazioneInputView(), typeof(IViewFor<PostazioneAddViewModel>));
            Locator.CurrentMutable.Register(() => new PostazioneInputView(), typeof(IViewFor<PostazioneDelViewModel>));
            Locator.CurrentMutable.Register(() => new PostazioneInputView(), typeof(IViewFor<PostazioneUpdViewModel>));

            Locator.CurrentMutable.Register(() => new RepartiView(), typeof(IViewFor<RepartiViewModel>));

            Locator.CurrentMutable.Register(() => new SettoreGroupView(), typeof(IViewFor<SettoreGroupViewModel>));
            Locator.CurrentMutable.Register(() => new SettoreInputView(), typeof(IViewFor<SettoreAddViewModel>));
            Locator.CurrentMutable.Register(() => new SettoreInputView(), typeof(IViewFor<SettoreDelViewModel>));
            Locator.CurrentMutable.Register(() => new SettoreInputView(), typeof(IViewFor<SettoreUpdViewModel>));

            Locator.CurrentMutable.Register(() => new ListinoView(), typeof(IViewFor<ListinoViewModel>));


            Locator.CurrentMutable.Register(() => new TariffaGroupView(), typeof(IViewFor<TariffaGroupViewModel>));
            Locator.CurrentMutable.Register(() => new TariffaInputView(), typeof(IViewFor<TariffaAddViewModel>));
            Locator.CurrentMutable.Register(() => new TariffaInputView(), typeof(IViewFor<TariffaDelViewModel>));
            Locator.CurrentMutable.Register(() => new TariffaInputView(), typeof(IViewFor<TariffaUpdViewModel>));

        }
    }
}
