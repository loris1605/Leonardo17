using Servizi.ViewModels;
using Views;

namespace Servizi.Views
{
    public partial class AbbonamentoGroupView : BaseUserControl<AbbonamentoGroupViewModel>
    {
        protected override string RootControlName => "MainGrid";

        public AbbonamentoGroupView()
        {
            InitializeComponent();
        }
    }
}