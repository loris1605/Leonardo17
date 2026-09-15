using Avalonia.Input;
using Cassa.ViewModels;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
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
            var vm = this.ViewModel;

            
        });
    }
}