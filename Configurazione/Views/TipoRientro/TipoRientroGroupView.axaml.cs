using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Threading;
using Configurazione.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Views;

namespace Configurazione.Views;

public partial class TipoRientroGroupView : BaseUserControl<TipoRientroGroupViewModel>
{
    protected override string RootControlName => "MainGrid";

    public TipoRientroGroupView()
    {
        InitializeComponent();
    }
}