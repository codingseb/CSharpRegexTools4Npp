using System.Windows;
using System.Windows.Interactivity;

namespace RegexDialog.Behaviors
{
    public class SimplePropertyBindingBehavior : Behavior<DependencyObject>
    {
        private readonly ExpressionEvaluator expressionEvaluator = new ExpressionEvaluator();

        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register("PropertyName", typeof(string), typeof(SimplePropertyBindingBehavior), new PropertyMetadata(string.Empty, DependencyPropertyChanged));

        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(SimplePropertyBindingBehavior), new PropertyMetadata(null, DependencyPropertyChanged));

        private static void DependencyPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (dependencyObject is SimplePropertyBindingBehavior simplePropertyBinding)
            {
                simplePropertyBinding.UpdateValue();
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            UpdateValue();
        }

        private void UpdateValue()
        {
            if (!PropertyName.Equals(string.Empty))
            {
                try
                {
                    expressionEvaluator.Variables["obj"] = AssociatedObject;
                    expressionEvaluator.Variables["value"] = Value; ;

                    expressionEvaluator.Evaluate($"obj.{PropertyName} = value");
                }
                catch { }
            }
        }
    }
}
