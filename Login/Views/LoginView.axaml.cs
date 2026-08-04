using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Login.ViewModels;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Views;

namespace Login.Views;

public partial class LoginView : BaseUserControl<LoginViewModel>
{
    protected override string RootControlName => "RootGrid";

    public LoginView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            // Conserviamo il CompositeDisposable corrente per il ViewModel attualmente agganciato
            CompositeDisposable? currentVmDisposables = null;

            // BLINDARE L'ATTIVAZIONE: Eseguiamo i binding e i comandi solo quando
            // il ViewModel è realmente presente e agganciato alla View
            this.WhenAnyValue(x => x.ViewModel)
                .Where(vm => vm is not null)
                .Subscribe(vmObj =>
                {
                    // Dispose del precedente (se esiste) per evitare accumulo di sottoscrizioni
                    currentVmDisposables?.Dispose();

                    // Nuovo container per le sottoscrizioni legate al nuovo ViewModel
                    currentVmDisposables = new CompositeDisposable();

                    // sicuro: vmObj non è null qui
                    var vm = vmObj!;

                    // 1. Gestione Focus Interaction
                    vm.PasswordFocus
                        .RegisterHandler(async interaction =>
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                PasswordBox.Focus();
                                PasswordBox.SelectAll();
                            });
                            interaction.SetOutput(Unit.Default);
                        })
                        .DisposeWith(currentVmDisposables);

                    // 2. Gestione Tasto ESCAPE
                    Observable.FromEventPattern<KeyEventArgs>(this, nameof(this.KeyDown))
                        .Where(e => e.EventArgs.Key == Key.Escape)
                        .Select(_ => Unit.Default)
                        .InvokeCommand(vm.EscPressedCommand)
                        .DisposeWith(currentVmDisposables);

                    // 3. Gestione Tasto ENTER sulla PasswordBox (Bypass del blocco nativo)
                    var target = PasswordBox;
                    var enterSequence = Observable.Create<EventPattern<KeyEventArgs>>(observer =>
                    {
                        if (target is null)
                        {
                            // Se il controllo non è presente, non sottoscriviamo nulla
                            return Disposable.Empty;
                        }

#pragma warning disable IDE0039 // Usa la funzione locale
                        EventHandler<KeyEventArgs> handler = (s, e) => observer.OnNext(new EventPattern<KeyEventArgs>(s, e));
#pragma warning restore IDE0039 // Usa la funzione locale

                        // Aggiungiamo l'handler in modo esplicito
                        target.AddHandler(InputElement.KeyDownEvent, handler, RoutingStrategies.Tunnel, true);

                        return Disposable.Create(() =>
                        {
                            try
                            {
                                target.RemoveHandler(InputElement.KeyDownEvent, handler);
                            }
                            catch
                            {
                                // ignora errori di rimozione (control potrebbe essere già distrutto)
                            }
                        });
                    });

                    enterSequence
                        .Where(e => e.EventArgs.Key == Key.Enter)
                        .Select(_ => Unit.Default)
                        .InvokeCommand(vm.SaveCommand) // uso di vm non-null
                        .DisposeWith(currentVmDisposables);

                    // 4. BINDING REATTIVI
                    this.Bind(vm, viewModel => viewModel.PasswordText, view => view.PasswordBox.Text)
                        .DisposeWith(currentVmDisposables);

                    this.Bind(vm, viewModel => viewModel.BindingT, view => view.OperatoreCombo.SelectedItem)
                        .DisposeWith(currentVmDisposables);

                    // Assicuriamo che le sottoscrizioni legate al VM vengano rimosse
                    // quando la view viene disattivata (d è il disposable fornito da WhenActivated)
                    currentVmDisposables.DisposeWith(d);
                })
                .DisposeWith(d);

            // 5. EVENTO COMBO BOX (Indipendente dal ViewModel, legato solo alla View)
            Observable.FromEventPattern<EventHandler, EventArgs>(
                        h => OperatoreCombo.DropDownClosed += h,
                        h => OperatoreCombo.DropDownClosed -= h)
                .Select(_ => Unit.Default)
                .Subscribe(async _ =>
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        PasswordBox.Focus();
                        PasswordBox.SelectAll();
                    });
                })
                .DisposeWith(d);
        });
    }
}