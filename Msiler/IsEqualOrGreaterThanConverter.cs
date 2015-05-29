using System;
using System.Globalization;
using System.Windows.Data;

namespace Quart.Msiler
{
    public class IsGreaterThanZero : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) {
                return false;
            }
            try {
                return Int32.Parse((string)value) > 0;
            } catch {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
