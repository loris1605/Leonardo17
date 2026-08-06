using Menu.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Views;

namespace Menu.Views;

public partial class MenuView : BaseUserControl<MenuViewModel>
{
    protected override string RootControlName => "RootGrid";

    public MenuView()
    {

        InitializeComponent();

        this.WhenActivated(d =>
        {
            // Contenitore per le sottoscrizioni legate al ViewModel corrente
            CompositeDisposable currentVmDisposables = null;

            // Osserviamo il ViewModel: quando cambia, smaltiamo le vecchie sottoscrizioni e ne creiamo di nuove
            this.WhenAnyValue(x => x.ViewModel)
                .Where(vm => vm is not null)
                .Subscribe(vmObj =>
                {
                    // Dispose del precedente (se esiste) per evitare accumulo di sottoscrizioni
                    currentVmDisposables?.Dispose();

                    // Nuovo container per le sottoscrizioni legate al nuovo ViewModel
                    currentVmDisposables = new CompositeDisposable();

                    // Sicuro: vmObj non è null qui
                    var vm = vmObj!;

                    // One-way bindings (usando l'istanza del ViewModel per rimuovere warning nullable)
                    this.OneWayBind(vm, v => v.AmministratoreVisible, vctrl => vctrl.SociItem.IsVisible)
                        .DisposeWith(currentVmDisposables);

                    this.OneWayBind(vm, v => v.ChiudiGiornataEnabled, vctrl => vctrl.CassaItem.IsEnabled)
                        .DisposeWith(currentVmDisposables);
                    //this.OneWayBind(vm, v => v.BarVisible, vctrl => vctrl.BarItem.IsVisible)
                    //    .DisposeWith(currentVmDisposables);
                    //this.OneWayBind(vm, v => v.ChiudiGiornataEnabled, vctrl => vctrl.BarItem.IsEnabled)
                    //    .DisposeWith(currentVmDisposables);
                    //this.OneWayBind(vm, v => v.PulizieVisible, vctrl => vctrl.PulizieItem.IsVisible)
                    //    .DisposeWith(currentVmDisposables);
                    //this.OneWayBind(vm, v => v.ChiudiGiornataEnabled, vctrl => vctrl.PulizieItem.IsEnabled)
                    //    .DisposeWith(currentVmDisposables);
                    //this.OneWayBind(vm, v => v.GuardarobaVisible, vctrl => vctrl.GuardarobaItem.IsVisible)
                    //    .DisposeWith(currentVmDisposables);
                    //this.OneWayBind(vm, v => v.ChiudiGiornataEnabled, vctrl => vctrl.GuardarobaItem.IsEnabled)
                    //    .DisposeWith(currentVmDisposables);
                    //this.OneWayBind(vm, v => v.ReportVisible, vctrl => vctrl.ReportItem.IsVisible)
                    //    .DisposeWith(currentVmDisposables);
                    this.OneWayBind(vm, v => v.ApriGiornataEnabled, vctrl => vctrl.ApriGiornataItem.IsEnabled)
                        .DisposeWith(currentVmDisposables);
                    this.OneWayBind(vm, v => v.ChiudiGiornataEnabled, vctrl => vctrl.ChiudiGiornataItem.IsEnabled)
                        .DisposeWith(currentVmDisposables);
                    //this.OneWayBind(vm, v => v.ChiudiGiornataEnabled, vctrl => vctrl.ApriTurnoItem.IsEnabled)
                    //    .DisposeWith(currentVmDisposables);
                    //this.OneWayBind(vm, v => v.ChiudiGiornataEnabled, vctrl => vctrl.ChiudiTurnoItem.IsEnabled)
                    //    .DisposeWith(currentVmDisposables);
                    this.OneWayBind(vm, v => v.IsMenuReady, vctrl => vctrl.MainMenu.IsVisible)
                        .DisposeWith(currentVmDisposables);

                    // Dispose del container del VM quando la view viene disattivata
                    currentVmDisposables.DisposeWith(d);
                })
                .DisposeWith(d);

        });

    }

}

            // eventuali binding della View non legati al ViewModel rimangono qui