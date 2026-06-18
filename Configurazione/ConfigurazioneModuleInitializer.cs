using Configurazione.Core.Context;
using Configurazione.Core.Repository;
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

            // CORRETTO: Spostiamo il GetService dentro l'ambito della Lambda () => ...
            // In questo modo, il DbContext verrà cercato solo quando verrà creato il Repository
            Locator.CurrentMutable.Register(() =>
            {
                var context = Locator.Current.GetService<IOperatoreDbContext>();
                return new ConfigurazioneOperatoreRepository(context);
            }, typeof(IConfigurazioneOperatoreRepository));
        }
    }
}
