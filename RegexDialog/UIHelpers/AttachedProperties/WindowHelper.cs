using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RegexDialog
{
    public static class WindowHelper
    {
        public static Key GetCloseOnKey(DependencyObject obj)
        {
            return (Key)obj.GetValue(CloseOnKeyProperty);
        }

        public static void SetCloseOnKey(DependencyObject obj, Key value)
        {
            obj.SetValue(CloseOnKeyProperty, value);
        }

        public static readonly DependencyProperty CloseOnKeyProperty =
            DependencyProperty.RegisterAttached("CloseOnKey", typeof(Key), typeof(WindowHelper), new PropertyMetadata(Key.None, OnCloseOnKeyChanged));

        private static void OnCloseOnKeyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is Window window)
            {
                if ((Key)e.OldValue == Key.None && (Key)e.NewValue != Key.None)
                    window.PreviewKeyDown += Windows_KeyDetection_PreviewKeyDown;
                else if ((Key)e.NewValue == Key.None && (Key)e.OldValue != Key.None)
                    window.PreviewKeyDown -= Windows_KeyDetection_PreviewKeyDown;
            }
        }

        private static void Windows_KeyDetection_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is Window window && e.Key == GetCloseOnKey(window))
            {
                if (Keyboard.FocusedElement is TextBox textBox
                    && TextBoxHelper.GetClearOnKey(textBox).Equals(GetCloseOnKey(window)))
                {
                    TextBoxHelper.TextBox_KeyDetection_PreviewKeyDown(textBox, e);
                }

                if (!e.Handled)
                {
                    window.Close();
                    e.Handled = true;
                }
            }
        }
    }
}
