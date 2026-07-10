using Cassa.ViewModels;
using Views;

namespace Cassa.Views;

public partial class CassaPostazioneView : BaseUserControl<CassaViewModel>
{
    protected override string RootControlName => "MainGrid";

    public CassaPostazioneView()
    {
        InitializeComponent();
    }
}