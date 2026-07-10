using Cassa.ViewModels;
using Views;

namespace Cassa.Views;

public partial class CassaView : BaseUserControl<CassaViewModel>
{
    protected override string RootControlName => "MainGrid";

    public CassaView()
    {
        InitializeComponent();
    }
}