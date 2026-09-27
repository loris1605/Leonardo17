using Avalonia;
using Avalonia.Input;
using ReactiveUI;
using Servizi.ViewModels;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Views;

namespace Servizi.Views
{
    public partial class AbbonamentoInputView : BaseUserControl<AbbonamentoInputBase>,
                                        IViewFor<AbbonamentoAddViewModel>,
                                        IViewFor<AbbonamentoUpdViewModel>,
                                        IViewFor<AbbonamentoDelViewModel>
    {
        protected override string RootControlName => "MainGrid";
        AbbonamentoAddViewModel IViewFor<AbbonamentoAddViewModel>.ViewModel
        {
            get => ViewModel as AbbonamentoAddViewModel;
            set => ViewModel = value;
        }

        AbbonamentoUpdViewModel IViewFor<AbbonamentoUpdViewModel>.ViewModel
        {
            get => ViewModel as AbbonamentoUpdViewModel;
            set => ViewModel = value;
        }

        AbbonamentoDelViewModel IViewFor<AbbonamentoDelViewModel>.ViewModel
        {
            get => ViewModel as AbbonamentoDelViewModel;
            set => ViewModel = value;
        }

        public AbbonamentoInputView()
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
                          vm => vm.BindingT.NomeAbbonamento,
                          v => v.NomeBox.Text)
                    .DisposeWith(d);

                //Bind Label to TextBox
                this.Bind(ViewModel,
                          vm => vm.BindingT.NumeroIngressi,
                          v => v.IngressiBox.Text)
                    .DisposeWith(d);

                this.Bind(ViewModel,
                          vm => vm.BindingT.DurataGiorni,
                          v => v.DurataBox.Text)
                    .DisposeWith(d);

                this.Bind(ViewModel,
                          vm => vm.BindingT.Prezzo,
                          v => v.PrezzoBox.Value)
                    .DisposeWith(d);

                this.Bind(ViewModel,
                          vm => vm.BindingT.Attivo,
                          v => v.AbilitatoCheckBox.IsChecked)
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
}