using Avalonia.Input;
using Configurazione.ViewModels;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Views;

namespace Configurazione.Views;


public partial class TipoRientroInputView : BaseUserControl<TipoRientroInputBase>,
                                        IViewFor<TipoRientroAddViewModel>,
                                        IViewFor<TipoRientroUpdViewModel>,
                                        IViewFor<TipoRientroDelViewModel>
{

    protected override string RootControlName => "MainGrid";

    TipoRientroAddViewModel IViewFor<TipoRientroAddViewModel>.ViewModel
    {
        get => ViewModel as TipoRientroAddViewModel;
        set => ViewModel = value;
    }

    TipoRientroUpdViewModel IViewFor<TipoRientroUpdViewModel>.ViewModel
    {
        get => ViewModel as TipoRientroUpdViewModel ;
        set => ViewModel = value;
    }

    TipoRientroDelViewModel IViewFor<TipoRientroDelViewModel>.ViewModel
    {
        get => ViewModel as TipoRientroDelViewModel;
        set => ViewModel = value;
    }

    public TipoRientroInputView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            ViewModel?.NomeFocus
                    .RegisterHandler(interaction =>
                    {
                        NomeBox.Focus();
                        NomeBox.SelectAll();
                        interaction.SetOutput(Unit.Default);
                    })
                    .DisposeWith(d);

            
            this.OneWayBind(ViewModel,
                    vm => vm.EscFocus,
                    view => view.InputSaveBox.EscFocus)
                .DisposeWith(d);

            // Esc Key Pressed
            Observable.FromEventPattern<EventHandler<KeyEventArgs>, KeyEventArgs>(
                        h => this.KeyUp += h,
                        h => this.KeyUp -= h)
            .Where(e => e.EventArgs.Key == Key.Escape)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Select(_ => Unit.Default) // Il comando si aspetta Unit
            .InvokeCommand(ViewModel, x => x.EscPressedCommand)
            .DisposeWith(d);

            //Bind Nome to TextBox
            this.Bind(ViewModel,
                      vm => vm.BindingT.Nome,
                      v => v.NomeBox.Text)
                .DisposeWith(d);

            
            this.Bind(ViewModel,
                      vm => vm.BindingT.DurataOre,
                      v => v.DurataBox.Text)
                .DisposeWith(d);

            #region OneWay

            this.OneWayBind(ViewModel,
                    vm => vm.Titolo,
                    v => v.lblTitolo.Text)
            .DisposeWith(d);

            this.OneWayBind(ViewModel,
                    vm => vm.FieldsEnabled,
                    v => v.InputGrid.IsEnabled)
            .DisposeWith(d);


            this.OneWayBind(ViewModel,
                    vm => vm.InfoLabel,
                    v => v.InfoLabel.Text)
            .DisposeWith(d);

            #endregion

        });
    }
}