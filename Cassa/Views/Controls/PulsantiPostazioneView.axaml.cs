using Avalonia.Input;
using Cassa.ViewModels;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Views;

namespace Cassa.Views
{
    public partial class PulsantiPostazioneView : BaseUserControl<CassaPostazioneViewModel>
    {
        public PulsantiPostazioneView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {

                ViewModel?.PosizioneFocus
                .RegisterHandler(interaction =>
                {
                    PosizioneTextBox.Focus();
                    PosizioneTextBox.SelectAll();
                    interaction.SetOutput(Unit.Default);
                })
                .DisposeWith(d);

                // Posizione Enter Key Pressed
                this.WhenAnyValue(x => x.PosizioneTextBox)
                    .WhereNotNull()
                    .SelectMany(txt =>
                        // Creiamo l'osservabile manualmente sull'istanza del controllo
                        Observable.FromEventPattern<EventHandler<KeyEventArgs>, KeyEventArgs>(
                            h => txt.KeyUp += h,
                            h => txt.KeyUp -= h)
                    )
                    .Where(e => e.EventArgs.Key == Key.Enter) // Filtro sul tasto Enter
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Select(_ => Unit.Default)
                    .InvokeCommand(ViewModel, vm => vm.PosizioneEnterCommand)
                    .DisposeWith(d);

                // Posizione Esc Key Pressed
                this.WhenAnyValue(x => x.PosizioneTextBox)
                    .WhereNotNull()
                    .SelectMany(txt =>
                        // Creiamo l'osservabile manualmente sull'istanza del controllo
                        Observable.FromEventPattern<EventHandler<KeyEventArgs>, KeyEventArgs>(
                            h => txt.KeyUp += h,
                            h => txt.KeyUp -= h)
                    )
                    .Where(e => e.EventArgs.Key == Key.Escape) // Filtro sul tasto Enter
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Select(_ => Unit.Default)
                    .InvokeCommand(ViewModel, vm => vm.PosizioneEscCommand)
                    .DisposeWith(d);


                #region Twoways

                this.Bind(ViewModel,
                          vm => vm.BindingT.Posizione,
                          v => v.PosizioneTextBox.Text)
                    .DisposeWith(d);

                #endregion


            });

            
        }

        protected override string RootControlName => "MainGrid";
    }
}