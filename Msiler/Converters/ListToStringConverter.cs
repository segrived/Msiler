using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Msiler.Converters
{
    [ValueConversion(typeof(List<string>), typeof(string))]
    public class ListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (targetType != typeof(string))
                throw new InvalidOperationException("The target must be a String");
            var separator = (parameter == null) ? ", " : (string)parameter;
            return String.Join(separator, (List<string>)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
