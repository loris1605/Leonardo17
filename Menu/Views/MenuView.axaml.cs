using Avalonia.Controls;
using Menu.ViewModels;
using Menu.ViewModels.Map;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;
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
            #region TwoWay

            this.BindCommand(ViewModel,
                vm => vm.CassaPostazioneCommand,
                v => v.CassaPostazioneItem)
                .DisposeWith(d);

            #endregion

            #region OneWay

            this.OneWayBind(ViewModel,
                            vm => vm.AmministratoreVisible,
                            v => v.AmministratoreItem.IsVisible)
                .DisposeWith(d);

            this.OneWayBind(ViewModel,
                            vm => vm.CassaVisible,
                            v => v.CassaItem.IsVisible)
                .DisposeWith(d);

            this.OneWayBind(ViewModel,
                            vm => vm.ChiudiGiornataEnabled,
                            v => v.CassaItem.IsEnabled)
                .DisposeWith(d);

            this.OneWayBind(ViewModel,
                            vm => vm.BarVisible,
                            v => v.BarItem.IsVisible)
                .DisposeWith(d);

            this.OneWayBind(ViewModel,
                            vm => vm.ChiudiGiornataEnabled,
                            v => v.BarItem.IsEnabled)
                .DisposeWith(d);

            this.OneWayBind(ViewModel,
                            vm => vm.PulizieVisible,
                            v => v.PulizieItem.IsVisible)
                .DisposeWith(d);

            this.OneWayBind(ViewModel,
                            vm => vm.ChiudiGiornataEnabled,
                            v => v.PulizieItem.IsEnabled)
                .DisposeWith(d);

            this.OneWayBind(ViewModel,
                            vm => vm.GuardarobaVisible,
                            v => v.GuardarobaItem.IsVisible)
                .DisposeWith(d);

            this.OneWayBind(ViewModel,
                            vm => vm.ChiudiGiornataEnabled,
                            v => v.GuardarobaItem.IsEnabled)
                .DisposeWith(d);

            this.OneWayBind(ViewModel,
                            vm => vm.ReportVisible,
                            v => v.ReportItem.IsVisible)
                .DisposeWith(d);

            this.OneWayBind(ViewModel,
                            vm => vm.ApriGiornataEnabled,
                            v => v.ApriGiornataItem.IsEnabled)
                .DisposeWith(d);

            this.OneWayBind(ViewModel,
                            vm => vm.ChiudiGiornataEnabled,
                            v => v.ChiudiGiornataItem.IsEnabled)
                .DisposeWith(d);

            this.OneWayBind(ViewModel,
                            vm => vm.ChiudiGiornataEnabled,
                            v => v.ApriTurnoItem.IsEnabled)
                .DisposeWith(d);

            this.OneWayBind(ViewModel,
                            vm => vm.ChiudiGiornataEnabled,
                            v => v.ChiudiTurnoItem.IsEnabled)
                .DisposeWith(d);

            this.OneWayBind(ViewModel,
                            vm => vm.IsMenuReady,
                            v => v.MainMenu.IsVisible)
                .DisposeWith(d);

            #endregion

            // 4. BINDING COMANDI (Se non fatti in XAML)
            //this.Bind(ViewModel, vm => vm.LogoutCommand, v => v.Title.ExitCommand).DisposeWith(d);

            
            

        });


    }

    private void OnChildMenuItemDataContextChanged(object sender, EventArgs e)
    {
        // 1. Verifichiamo che il mittente sia il sotto-menu MenuItem
        if (sender is MenuItem childMenuItem)
        {
            // 2. Recuperiamo il record specifico della riga corrente
            if (childMenuItem.DataContext is MenuPostazioneMap postazioneData)
            {
                // 3. Risaliamo l'albero logico per trovare il ViewModel principale della pagina
                // CassaPostazioneItem è il nome memorizzato tramite x:Name nel file XAML

                if (CassaPostazioneItem.DataContext is MenuViewModel mainViewModel)
                {
                    // 4. Assegniamo comando e parametro direttamente da C#
                    childMenuItem.Command = mainViewModel.CassaPostazioneCommand;
                    childMenuItem.CommandParameter = postazioneData.CodicePostazione;
                }
            }
        }
    }


}