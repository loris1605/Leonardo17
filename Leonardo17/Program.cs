using System;
using System.Reactive.Concurrency;
using Avalonia;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace Leonardo17
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            // Usiamo lo scheduler centralizzato per catturare l'errore in modo sicuro
            RxSchedulers.MainThreadScheduler.Schedule(() =>
            {
                // Ci mettiamo in ascolto degli errori non gestiti registrati sulla nostra infrastruttura reattiva
                System.AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    if (e.ExceptionObject is Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("====================================================");
                        System.Diagnostics.Debug.WriteLine($"🚨 CRASH REATTIVO INTERCETTATO (RxSchedulers): {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Origine: {ex.TargetSite}");
                        System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                        if (ex.InnerException != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                        }
                        System.Diagnostics.Debug.WriteLine("====================================================");
                    }
                };
            });

            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
#if DEBUG
                .WithDeveloperTools()
#endif
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI(_ => { });
    }
}
