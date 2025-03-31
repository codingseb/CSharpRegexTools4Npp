using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RegexDialog.Converters
{
    public class EnumBooleanConverter : BaseConverter, IValueConverter
    {
        public bool InverseBool { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string parameterString))
                return DependencyProperty.UnsetValue;

            if (!Enum.IsDefined(value.GetType(), value))
                return DependencyProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return InverseBool ? !parameterValue.Equals(value) : parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string parameterString) || !(bool)value)
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
    }
}
