using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RegexDialog
{
    public static class TextBoxHelper
    {
        public static Key GetClearOnKey(DependencyObject obj)
        {
            return (Key)obj.GetValue(ClearOnKeyProperty);
        }

        public static void SetClearOnKey(DependencyObject obj, Key value)
        {
            obj.SetValue(ClearOnKeyProperty, value);
        }

        // Using a DependencyProperty as the backing store for ClearOnKey.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClearOnKeyProperty =
            DependencyProperty.RegisterAttached("ClearOnKey", typeof(Key), typeof(TextBoxHelper), new PropertyMetadata(Key.None, OnClearOnKeyChanged));

        private static void OnClearOnKeyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is TextBox textBox)
            {
                if ((Key)e.OldValue == Key.None && (Key)e.NewValue != Key.None)
                    textBox.PreviewKeyDown += TextBox_KeyDetection_PreviewKeyDown;
                else if ((Key)e.NewValue == Key.None && (Key)e.OldValue != Key.None)
                    textBox.PreviewKeyDown -= TextBox_KeyDetection_PreviewKeyDown;
            }
        }

        internal static void TextBox_KeyDetection_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox
                && e.Key == GetClearOnKey(textBox)
                && !string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = string.Empty;
                e.Handled = true;
            }
        }
    }
}
