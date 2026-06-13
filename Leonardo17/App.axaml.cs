using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Core.Context;
using Core.Repository;
using Splat;

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
            RegisterCoreServices();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void RegisterCoreServices()
        {
            Locator.CurrentMutable.Register(() => new SettingDbContext(), typeof(ISettingDbContext));
            Locator.CurrentMutable.Register(() => new SettingRepository(
                Locator.Current.GetService<ISettingDbContext>()), typeof(ISettingRepository));
        }
    }
}