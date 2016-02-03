using System;
using System.Globalization;
using System.Windows.Data;

namespace Msiler.Converters
{
    public class IsGreaterThenConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new IsGreaterThenConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (int)value > Int32.Parse((string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
