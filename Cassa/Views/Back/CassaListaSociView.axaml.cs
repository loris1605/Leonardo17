using Cassa.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;
using Views;

namespace Cassa.Views
{
    public partial class CassaListaSociView : BaseUserControl<CassaListaSociViewModel>
    {
        protected override string RootControlName => "MainGrid";

        public CassaListaSociView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {



                #region OneWay

                this.OneWayBind(ViewModel,
                                vm => vm.Titolo,
                                v => v.Title.TitoloPagina)
                    .DisposeWith(d);

                

                #endregion

            });

        }

    }
}