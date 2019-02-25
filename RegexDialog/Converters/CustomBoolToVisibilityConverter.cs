using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RegexDialog.Converters
{
    /// <summary>
    /// Converter that convert bool to Visibility the way you want.
    /// The two Properties VisibilityForTrueValue and VisibilityForFalseValue are use to know how to map Visibility and Boolean values.
    /// </summary>
    public class CustomBoolToVisibilityConverter : BaseConverter, IValueConverter
    {
        public Visibility? InDesigner { get; set; } = null;

        /// <summary>
        /// The Value of the visibility when the source value is DependencyProperty.UnsetValue
        /// Default is Visibility.Collapsed
        /// </summary>
        public Visibility OnUnsetValue { get; set; } = Visibility.Collapsed;

        /// <summary>
        /// The Value of the visibility when the source value is null
        /// Default is Visibility.Collapsed
        /// </summary>
        public Visibility OnNullValue { get; set; } = Visibility.Collapsed;

        /// <summary>
        /// The Value of the visibility when the source value is true
        /// Default is Visibility.Visible
        /// </summary>
        public Visibility TrueValue { get; set; } = Visibility.Visible;

        /// <summary>
        /// The Value of the visibility when the source value is true
        /// Default is Visibility.Collapsed
        /// </summary>
        public Visibility FalseValue { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()) && InDesigner != null) return InDesigner;
            else if (value == null) return OnNullValue;
            else if (value == DependencyProperty.UnsetValue) return OnUnsetValue;
            

            return (value is bool && (bool)value ? TrueValue : FalseValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is Visibility && (Visibility)value == TrueValue);
        }
    }
}
