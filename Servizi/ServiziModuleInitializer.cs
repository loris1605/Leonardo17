using Servizi.Core.Context;
using Servizi.Core.Repository;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // Initialization code for the Servizi module
            Console.WriteLine("Servizi module initialized.");
        }
    }
}
