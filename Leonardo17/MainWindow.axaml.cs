using ReactiveUI;
using ReactiveUI.Avalonia;
using Splat;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using ViewModels;

namespace Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            ViewModel = Locator.Current.GetService<IMainWindowViewModel>() as MainWindowViewModel;


            if (ViewModel == null)
            {
                throw new InvalidOperationException("Impossibile risolvere MainWindowViewModel da Splat.");
            }

            Debug.WriteLine(">>> [MAIN] MainWindow inizializzata con ViewModel: " + ViewModel.GetType().Name);



            this.WhenActivated(disposables =>
            {
                // Gestore dell'evento definito localmente per poterlo rimuovere
                NotifyCollectionChangedEventHandler collectionChangedHandler = (s, e) =>
                {
                    // Forza la pulizia della memoria dopo la transizione della vista
                    Task.Delay(1000).ContinueWith(_ =>
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                        System.Diagnostics.Debug.WriteLine(">>> [MAIN] GC Forzato dopo cambio vista.");
                    });
                };

                // Ci iscriviamo all'evento del Router
                ViewModel.Router.NavigationStack.CollectionChanged += collectionChangedHandler;

                // CORREZIONE ANTI-MEMORY LEAK: 
                // Usiamo 'disposables' per dire a ReactiveUI di disiscriversi AUTOMATICAMENTE 
                // dall'evento quando la finestra viene disattivata o chiusa.
                Disposable.Create(() =>
                {
                    ViewModel.Router.NavigationStack.CollectionChanged -= collectionChangedHandler;
                })
                .DisposeWith(disposables);
            });
        }
    }
}