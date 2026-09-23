using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Servizi.ViewModels;
using System.Reactive.Disposables.Fluent;
using Views;

namespace Servizi.Views
{
    public partial class ServiziView : BaseUserControl<ServiziViewModel>
    {
        protected override string RootControlName => "MainGrid";

        public ServiziView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {


                #region OneWay

                this.OneWayBind(ViewModel,
                                vm => vm.GroupEnabled,
                                v => v.RouterHost.IsEnabled)
                    .DisposeWith(d);

                #endregion


            });
        }
    }
}