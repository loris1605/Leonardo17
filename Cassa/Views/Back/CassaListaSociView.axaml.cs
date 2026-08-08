using Cassa.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia.Input;
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

                // Double-click on DataGrid row -> return to postazione with selected posizione
                var doubleTapSub = Observable.FromEventPattern<EventHandler<TappedEventArgs>, TappedEventArgs>(
                        h => SociInCassaDataGrid.DoubleTapped += h,
                        h => SociInCassaDataGrid.DoubleTapped -= h)
                    .Select(_ => ViewModel?.BindingT)
                    .Where(map => map != null)
                    .Subscribe(map =>
                    {
                        // Trigger viewmodel to notify host and close this view
                        ViewModel?.ReturnToPostazione(map!.Posizione);
                    });

                doubleTapSub.DisposeWith(d);

            });

        }

    }
}