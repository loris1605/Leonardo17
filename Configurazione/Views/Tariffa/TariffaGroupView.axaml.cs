using Configurazione.ViewModels;
using Views;

namespace Configurazione.Views;

public partial class TariffaGroupView : BaseUserControl<TariffaGroupViewModel>
{
    protected override string RootControlName => "MainGrid";

    public TariffaGroupView()
    {
        InitializeComponent();

    }

}