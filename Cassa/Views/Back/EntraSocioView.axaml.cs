using Cassa.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;
using Views;

namespace Cassa.Views;

public partial class EntraSocioView : BaseUserControl<EntraSocioViewModel>
{
    protected override string RootControlName => "MainGrid";

    public EntraSocioView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {

            this.OneWayBind(ViewModel,
                vm => vm.TesseraFocus,
                view => view.AnagraficaInput.TesseraFocus)
            .DisposeWith(d);

            this.OneWayBind(ViewModel,
                    vm => vm.CanEntraLabel,
                    v => v.CanEntraLabel.Text)
            .DisposeWith(d);
        });
    }
}