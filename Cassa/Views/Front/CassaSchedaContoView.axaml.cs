using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Cassa.ViewModels;
using Views;

namespace Cassa.Views
{
    public partial class CassaSchedaContoView : BaseUserControl<SchedaContoViewModel>
    {
        protected override string RootControlName => "MainGrid";

        public CassaSchedaContoView()
        {
            InitializeComponent();
        }
    }
}