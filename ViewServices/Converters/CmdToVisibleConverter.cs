using Avalonia.Data.Converters;
using System.Globalization;
using System.Windows.Input;

namespace ViewServices
{
    public class CmdToVisibleConverter : IValueConverter
    {
        // Restituisce true se l'oggetto è un ICommand e CanExecute(null) == true.
        // Se non è ICommand ritorna false.
        // Nota: in XAML puoi passare il parametro al CanExecute tramite ConverterParameter.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICommand cmd)
            {
                try
                {
                    // Passiamo il parameter del converter al CanExecute se presente, altrimenti null
                    return cmd.CanExecute(parameter);
                }
                catch
                {
                    // In caso di eccezione ritornare true per non nascondere erroneamente la UI
                    return true;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
