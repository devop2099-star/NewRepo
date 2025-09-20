using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Naviguard.Utils.Converters
{
    /// <summary>
    /// Convierte un valor booleano a un valor de Visibilidad, pero de forma inversa.
    /// true -> Collapsed
    /// false -> Visible
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibilityValue)
            {
                return visibilityValue == Visibility.Collapsed;
            }
            return false;
        }
    }
}
