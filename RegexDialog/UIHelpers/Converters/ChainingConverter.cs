using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace RegexDialog.Converters
{
    /// <summary>
    /// This Converter is a master converter that chain 2 sub converters and convert through the chain.
    /// It can also chain an other ChainingConverter or two.
    /// Or it can take a list of converters to chain as Content of the converter
    /// </summary>
    [ContentProperty("Converters")]
    public class ChainingConverter : BaseConverter, IValueConverter
    {
        /// <summary>
        /// First Converter to chain (input converter)
        /// </summary>
        public IValueConverter Converter1 { get; set; }

        /// <summary>
        /// Second Converter to chain (output converter)
        /// </summary>
        public IValueConverter Converter2 { get; set; }

        public object Converter1Parameter { get; set; }
        public object Converter2Parameter { get; set; }

        /// <summary>
        /// For a list of converters to chain (Use as content Property, Converter1 and Converter2 must be null)
        /// </summary>
        public List<IValueConverter> Converters { get; } = new List<IValueConverter>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue)
            {
                return value;
            }
            else if (Converter1 == null || Converter2 == null)
            {
                foreach (var converter in Converters)
                {
                    value = converter.Convert(value, targetType, parameter, culture);
                    if (value == Binding.DoNothing) return Binding.DoNothing;
                    if (value == DependencyProperty.UnsetValue) return DependencyProperty.UnsetValue;
                }

                return value;
            }
            else
            {
                return Converter2.Convert(Converter1.Convert(value, targetType, Converter1Parameter ?? parameter, culture), targetType, Converter2Parameter ?? parameter, culture);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue)
            {
                return value;
            }
            else if (Converter1 == null || Converter2 == null)
            {
                List<IValueConverter> convertersReverseList = new List<IValueConverter>(Converters);
                convertersReverseList.Reverse();

                foreach (var converter in convertersReverseList)
                {
                    value = converter.ConvertBack(value, targetType, parameter, culture);
                    if (value == Binding.DoNothing) return Binding.DoNothing;
                    if (value == DependencyProperty.UnsetValue) return DependencyProperty.UnsetValue;
                }

                return value;
            }
            else
            {
                return Converter1.ConvertBack(Converter2.ConvertBack(value, targetType, parameter, culture), targetType, parameter, culture);
            }
        }
    }
}
