using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfApp10.Helpers
{
    public class BooleanNegationConverter : IValueConverter
    {
        public static readonly BooleanNegationConverter Default = new BooleanNegationConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;

            return false;
        }
    }
}