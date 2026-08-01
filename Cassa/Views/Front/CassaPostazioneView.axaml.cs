using Cassa.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;
using Views;

namespace Cassa.Views;

public partial class CassaPostazioneView : BaseUserControl<CassaPostazioneViewModel>
{
    protected override string RootControlName => "MainGrid";

    public CassaPostazioneView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {



            #region OneWay

            this.OneWayBind(ViewModel,
                            vm => vm.Titolo,
                            v => v.Title.TitoloPagina)
                .DisposeWith(d);

            this.OneWayBind(ViewModel,
                            vm => vm.IsOpen,
                            v => v. CognomeTextBlock.IsVisible)
                .DisposeWith(d);

            this.OneWayBind(ViewModel,
                            vm => vm.IsOpen,
                            v => v.NomeTextBlock.IsVisible)
                .DisposeWith(d);

            #endregion

            #region Twoways

            #endregion


        });
    }
}