using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Cassa.ViewModels;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using Views;

namespace Cassa.Views
{
    public partial class EntraSocioView2 : BaseUserControl<EntraSocioViewModel>
    {
        protected override string RootControlName => "MainGrid";

        public EntraSocioView2()
        {
            InitializeComponent();

            _tesseraTextBox = this.FindControl<TextBox>("TesseraTextBox");

            this.WhenActivated(disposables =>
            {
                // Bind tessera text and search button -> TesseraCommand
                // usa la property wrapper invece di accedere direttamente al TextBox
                this.Bind(ViewModel,
                          vm => vm.BindingT.NumeroTessera,
                          v => v.TesseraText)
                    .DisposeWith(disposables);

                //this.BindCommand(ViewModel,
                //                 vm => vm.AnagraficaViewModel.TesseraCommand,
                //                 v => v.TesseraSearchButton)
                //    .DisposeWith(disposables);

                //// Anagrafica fields (readonly)
                //this.OneWayBind(ViewModel,
                //                vm => vm.BindingT.Cognome,
                //                v => v.CognomeTextBlock.Text)
                //    .DisposeWith(disposables);

                //this.OneWayBind(ViewModel,
                //                vm => vm.BindingT.Nome,
                //                v => v.NomeTextBlock.Text)
                //    .DisposeWith(disposables);

                //this.OneWayBind(ViewModel,
                //                vm => vm.Eta,
                //                v => v.EtaTextBlock.Text)
                //    .DisposeWith(disposables);

                // Posizione
                //this.Bind(ViewModel,
                //          vm => vm.BindingT.Posizione,
                //          v => v.PosizioneTextBox.Text)
                //    .DisposeWith(disposables);

                //// Ingressi list and selection
                //this.OneWayBind(ViewModel,
                //                vm => vm.IngressiList,
                //                v => v.IngressiComboBox.Items)
                //    .DisposeWith(disposables);

                //this.Bind(ViewModel,
                //          vm => vm.SelectedIngresso,
                //          v => v.IngressiComboBox.SelectedItem)
                //    .DisposeWith(disposables);

                //// Buttons
                //this.BindCommand(ViewModel,
                //                 vm => vm.EntraCommand,
                //                 v => v.EntraButton)
                //    .DisposeWith(disposables);

                //this.BindCommand(ViewModel,
                //                 vm => vm.AnagraficaViewModel.ApriSchedaCommand,
                //                 v => v.ApriSchedaButton)
                //    .DisposeWith(disposables);

                //// Labels and state
                //this.OneWayBind(ViewModel,
                //                vm => vm.InfoLabel,
                //                v => v.InfoTextBlock.Text)
                //    .DisposeWith(disposables);

                //this.OneWayBind(ViewModel,
                //                vm => vm.ErrorText,
                //                v => v.ErrorTextBlock.Text)
                //    .DisposeWith(disposables);

                //this.OneWayBind(ViewModel,
                //                vm => vm.CanEntraLabel,
                //                v => v.CanEntraLabelBlock.Text)
                //    .DisposeWith(disposables);

                //// Disable form if blocked
                //this.OneWayBind(ViewModel,
                //                vm => vm.IsEntryFormBlocked,
                //                v => v.TesseraTextBox.IsEnabled,
                //                (blocked) => !blocked)
                //    .DisposeWith(disposables);

                // Posizione focus interaction
                if (ViewModel != null)
                {
                    ViewModel.PosizioneFocus.RegisterHandler(interaction =>
                    {
                        // Usa il nome di tipo completo per evitare ambiguità con l'istanza 'Dispatcher'
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            PosizioneTextBox.Focus();
                            interaction.SetOutput(Unit.Default);
                        });
                    }).DisposeWith(disposables);
                }
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            CognomeTextBlock = this.FindControl<TextBlock>("CognomeTextBlock");
            NomeTextBlock = this.FindControl<TextBlock>("NomeTextBlock");
            EtaTextBlock = this.FindControl<TextBlock>("EtaTextBlock");
            
        }

        // Controls references
        public TextBlock CognomeTextBlock { get; private set; }
        public TextBlock NomeTextBlock { get; private set; }
        public TextBlock EtaTextBlock { get; private set; }
        
    }

    public partial class EntraSocioView2
    {
        private TextBox _tesseraTextBox;

        public string TesseraText
        {
            get => _tesseraTextBox?.Text ?? string.Empty;
            set
            {
                if (_tesseraTextBox != null && _tesseraTextBox.Text != value)
                {
                    _tesseraTextBox.Text = value;
                }
            }
        }
    }
}