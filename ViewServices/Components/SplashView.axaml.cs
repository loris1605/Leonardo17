using Avalonia;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;
using ViewModels;

namespace Views
{
    public partial class SplashView : BaseUserControl<ViewModelBase>
    {
        protected override string RootControlName => "Root";

        public SplashView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                
            });
        }

        public static readonly StyledProperty<string> TextLine1Property =
        AvaloniaProperty.Register<SplashView, string>(nameof(TextLine1), defaultValue: string.Empty);

        public string TextLine1
        {
            get => GetValue(TextLine1Property);
            set => SetValue(TextLine1Property, value);
        }

        public static readonly StyledProperty<string> TextLine2Property =
        AvaloniaProperty.Register<SplashView, string>(nameof(TextLine2), defaultValue: string.Empty);

        public string TextLine2
        {
            get => GetValue(TextLine2Property);
            set => SetValue(TextLine2Property, value);
        }
    }
}