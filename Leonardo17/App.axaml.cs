using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Core.Context;
using Core.Repository;
using ReactiveUI;
using Splat;
using System;
using ViewModels;
using Views;

namespace Leonardo17
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // 1. REGISTRA I SERVIZI PER PRIMI (Così sono pronti per la UI)
            RegisterAppServices();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = Locator.Current.GetService<MainWindow>();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static void RegisterAppServices()
        {
            Locator.CurrentMutable.Register(() => new SettingDbContext(), typeof(ISettingDbContext));
            Locator.CurrentMutable.Register(() =>
            {
                var dbContext = Locator.Current.GetService<ISettingDbContext>();
                return new SettingRepository(dbContext);
            }, typeof(ISettingRepository));

            Locator.CurrentMutable.Register(() => new MainWindow(), typeof(MainWindow));

            Locator.CurrentMutable.Register(() =>
            {
                var settingRepo = Locator.Current.GetService<ISettingRepository>();
                return new MainWindowViewModel(settingRepo);
            }, typeof(IMainWindowViewModel));

            Locator.CurrentMutable.Register(() => Locator.Current.GetService<MainWindow>(), typeof(IViewFor<MainWindowViewModel>));

            Locator.CurrentMutable.RegisterLazySingleton<IScreen>(() =>
            {
                // Equivale a sp.GetRequiredService<IMainWindowViewModel>()
                var mainWindowVm = Locator.Current.GetService<IMainWindowViewModel>();

                if (mainWindowVm == null)
                {
                    throw new InvalidOperationException("IMainWindowViewModel non è registrato in Splat.");
                }

                return (IScreen)mainWindowVm;
            });


        }
    }
}