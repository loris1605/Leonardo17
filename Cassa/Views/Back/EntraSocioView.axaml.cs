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

            // 2. Gestione Tasto ESCAPE
            Observable.FromEventPattern<KeyEventArgs>(this, nameof(this.KeyDown))
                .Where(e => e.EventArgs.Key == Key.Escape)
                .Select(_ => Unit.Default)
                .InvokeCommand(vm.EscPressedCommand)
                .DisposeWith(d);

            this.OneWayBind(ViewModel,
                vm => vm.TesseraFocus,
                view => view.AnagraficaInput.TesseraFocus)
            .DisposeWith(d);

            this.OneWayBind(ViewModel,
                    vm => vm.CanEntraLabel,
                    v => v.CanEntraLabel.Text)
            .DisposeWith(d);
        });
    }
}