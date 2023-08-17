using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace RegexDialog.Converters
{
    internal class ShowOnOneLineConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.ToString().Replace("\r", "").Replace("\n", "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
