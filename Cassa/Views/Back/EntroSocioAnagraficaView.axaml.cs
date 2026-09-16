using Avalonia;
using Avalonia.Input;
using Cassa.ViewModels;
using Avalonia.Threading;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Views;

namespace Cassa.Views;

public partial class EntraSocioAnagraficaView : BaseUserControl<EntraSocioAnagraficaViewModel>
{
    protected override string RootControlName => "MainGrid";

    public EntraSocioAnagraficaView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {

            this.WhenAnyValue(x => x.ViewModel)
                .Where(vm => vm is not null)
                .Subscribe(vmObj =>
                {
                    // 1. Gestione Focus Interaction
                    vmObj.TesseraFocus
                        .RegisterHandler(async interaction =>
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                TesseraBox.Focus();
                                TesseraBox.SelectAll();
                            });
                            interaction.SetOutput(Unit.Default);
                        })
                        .DisposeWith(d);
                });

            this.WhenAnyValue(x => x.ViewModel)
                .Where(vm => vm is not null)
                .Subscribe(vmObj =>
                {
                    // 1. Gestione Focus Interaction
                    vmObj.PosizioneFocus
                        .RegisterHandler(async interaction =>
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                PosizioneBox.Focus();
                                PosizioneBox.SelectAll();
                            });
                            interaction.SetOutput(Unit.Default);
                        })
                        .DisposeWith(d);
                });



            // 1. Definisci il flusso sorgente centralizzato e rendilo condiviso (.Publish().RefCount())
            var isPosizioneEnabled = this.WhenAnyValue(
                    x => x.ViewModel.BindingT.NumeroSocio,
                    x => x.ViewModel.IsSocioInside,
                    (numeroSocio, isInside) => !string.IsNullOrWhiteSpace(numeroSocio) && !isInside)
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Publish()
                .RefCount();

            // 2. Lega il flusso direttamente a PosizioneBox
            isPosizioneEnabled
                .BindTo(this, v => v.PosizioneBox.IsEnabled)
                .DisposeWith(d);

            // 3. Inverti il flusso (Negazione logica) e legalo a TesseraBox
            isPosizioneEnabled
                .Select(enabled => !enabled)
                .BindTo(this, v => v.TesseraBox.IsEnabled)
                .DisposeWith(d);

            // 1. Definisci il flusso sorgente centralizzato e rendilo condiviso (.Publish().RefCount())
            var isApriSchedaVisible = this.WhenAnyValue(x => x.ViewModel.IsSocioInside)
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Publish()
                .RefCount();

            // 2. Lega il flusso direttamente a ApriSchedaButton
            isApriSchedaVisible
                .BindTo(this, v => v.ApriSchedaButton.IsVisible)
                .DisposeWith(d);

            // --- STREAM EVENTI TASTIERA ---
            var keyUpTesseraStream = Observable.FromEventPattern<EventHandler<KeyEventArgs>, KeyEventArgs>(
                        h => this.TesseraBox.KeyUp += h,
                        h => this.TesseraBox.KeyUp -= h)
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Publish()
                    .RefCount();

            // Esegui TesseraCommand su INVIO (Esegue solo se il ViewModel è istanziato)
            keyUpTesseraStream
                .Where(e => e.EventArgs.Key == Key.Enter)
                .Where(_ => ViewModel != null)
                .Select(_ => Unit.Default)
                .InvokeCommand(ViewModel!, x => x.TesseraCommand)
                .DisposeWith(d);

            // Esegui F5Command su F5 (Esegue solo se il ViewModel è istanziato)
            keyUpTesseraStream
                .Where(e => e.EventArgs.Key == Key.F5)
                .Where(_ => ViewModel != null)
                .Select(_ => Unit.Default)
                .InvokeCommand(ViewModel!, x => x.F5Command)
                .DisposeWith(d);

            var keyUpPosizioneStream = Observable.FromEventPattern<EventHandler<KeyEventArgs>, KeyEventArgs>(
                        h => this.PosizioneBox.KeyUp += h,
                        h => this.PosizioneBox.KeyUp -= h)
                    .ObserveOn(RxSchedulers.MainThreadScheduler)
                    .Publish()
                    .RefCount();

            keyUpPosizioneStream
                .Where(e => e.EventArgs.Key == Key.Escape)
                .Where(_ => ViewModel != null)
                .Select(_ => Unit.Default)
                .InvokeCommand(ViewModel!, x => x.PosizioneEscCommand)
                .DisposeWith(d);

        });
    }



    //public static readonly StyledProperty<Interaction<Unit, Unit>> TesseraFocusProperty =
    //    AvaloniaProperty.Register<EntraSocioAnagraficaView, Interaction<Unit, Unit>>(nameof(TesseraFocus));

    //public Interaction<Unit, Unit> TesseraFocus
    //{
    //    get => GetValue(TesseraFocusProperty);
    //    set => SetValue(TesseraFocusProperty, value);
    //}

    //public static readonly StyledProperty<Interaction<Unit, Unit>> PosizioneFocusProperty =
    //    AvaloniaProperty.Register<EntraSocioAnagraficaView, Interaction<Unit, Unit>>(nameof(PosizioneFocus));

    //public Interaction<Unit, Unit> PosizioneFocus
    //{
    //    get => GetValue(PosizioneFocusProperty);
    //    set => SetValue(PosizioneFocusProperty, value);
    //}
}