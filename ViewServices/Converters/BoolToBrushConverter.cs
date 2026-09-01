using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;

namespace ViewServices
{
    public class BoolToBrushConverter : IValueConverter
    {
        // Convert: true => green, false => gray (configurabile via parameter)
        // parameter (string) options:
        //   "GreenRed" => true:Green false:Red
        //   "GreenTransparent" => true:Green false:Transparent (default)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var trueBrush = Brushes.Green;
            var falseBrush = Brushes.Transparent;

            if (parameter is string p)
            {
                if (string.Equals(p, "GreenRed", StringComparison.OrdinalIgnoreCase))
                {
                    falseBrush = Brushes.Red;
                }
                else if (string.Equals(p, "GreenGray", StringComparison.OrdinalIgnoreCase))
                {
                    falseBrush = Brushes.Gray;
                }
                else if (string.Equals(p, "GreenTransparent", StringComparison.OrdinalIgnoreCase))
                {
                    falseBrush = Brushes.Transparent;
                }
            }

            if (value is bool b)
            {
                return b ? (IBrush)trueBrush : (IBrush)falseBrush;
            }

            // Fallback: non-boolean values -> false brush
            return (IBrush)falseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
