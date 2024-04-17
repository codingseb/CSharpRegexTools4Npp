using System;
using System.Windows;
using System.Windows.Interactivity;

namespace RegexDialog.Behaviors
{
    public class SimplePropertyBindingBehavior : Behavior<DependencyObject>
    {
        private readonly ExpressionEvaluator expressionEvaluator = new();
        private string oldEventName = string.Empty;
        private readonly EventHandler<EventArgs> eventHandler;

        public SimplePropertyBindingBehavior()
        {
            eventHandler = new EventHandler<EventArgs>(OnTriggerEvent);
        }

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
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(SimplePropertyBindingBehavior), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, DependencyPropertyChanged));

        /// <summary>
        /// Set to false to disable the update of the view from the viewModel
        /// </summary>
        public bool UpdateViewFromViewModel
        {
            get { return (bool)GetValue(UpdateViewFromViewModelProperty); }
            set { SetValue(UpdateViewFromViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpdateViewFromViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UpdateViewFromViewModelProperty =
            DependencyProperty.Register("UpdateViewFromViewModel", typeof(bool), typeof(SimplePropertyBindingBehavior), new PropertyMetadata(true));

        public string PropertyChangedTriggerEventName
        {
            get { return (string)GetValue(PropertyChangedTriggerEventNameProperty); }
            set { SetValue(PropertyChangedTriggerEventNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyChangedTriggerEventName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyChangedTriggerEventNameProperty =
            DependencyProperty.Register("PropertyChangedTriggerEventName", typeof(string), typeof(SimplePropertyBindingBehavior), new PropertyMetadata(string.Empty, PropertyChangedTriggerEventNameChanged));

        private static void PropertyChangedTriggerEventNameChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (dependencyObject is SimplePropertyBindingBehavior simplePropertyBinding)
            {
                simplePropertyBinding.TriggerEventSubscribe();
            }
        }

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

            TriggerEventSubscribe();
            UpdateValue();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            TriggerEventUnsubscribe();
        }

        private void TriggerEventSubscribe()
        {
            if (AssociatedObject != null)
            {
                TriggerEventUnsubscribe();
                if (!PropertyChangedTriggerEventName.Equals(string.Empty))
                {
                    oldEventName = PropertyChangedTriggerEventName;

                    //WeakEventManager<type Final De l'objet, EventArgs>.AddHandler(AssociatedObject, PropertyChangedTriggerEventName, OnTriggerEvent);
                    typeof(WeakEventManager<,>)
                        .MakeGenericType(AssociatedObject.GetType(), typeof(EventArgs))
                        .GetMethod("AddHandler")
                        .Invoke(null, new object[] { AssociatedObject, PropertyChangedTriggerEventName, eventHandler });
                }
            }
        }

        private void TriggerEventUnsubscribe()
        {
            if (!oldEventName.Equals(string.Empty))
            {
                //WeakEventManager<type Final De l'objet, EventArgs>.RemoveHandler(AssociatedObject, PropertyChangedTriggerEventName, OnTriggerEvent);
                typeof(WeakEventManager<,>)
                    .MakeGenericType(AssociatedObject.GetType(), typeof(EventArgs))
                    .GetMethod("RemoveHandler")
                    .Invoke(null, new object[] { AssociatedObject, oldEventName, eventHandler });
            }
        }

        private void OnTriggerEvent(object source, EventArgs args)
        {
            if (!PropertyName.Equals(string.Empty))
            {
                try
                {
                    expressionEvaluator.Variables["obj"] = AssociatedObject;

                    object result = expressionEvaluator.Evaluate($"obj.{PropertyName}");

                    if (!result.Equals(Value))
                        Value = result;
                }
                catch { }
            }
        }

        private void UpdateValue()
        {
            if (UpdateViewFromViewModel && AssociatedObject != null && !PropertyName.Equals(string.Empty))
            {
                try
                {
                    expressionEvaluator.Variables["obj"] = AssociatedObject;
                    object result = expressionEvaluator.Evaluate($"obj.{PropertyName}");

                    if (!result.Equals(Value))
                    {
                        expressionEvaluator.Variables["value"] = Value;

                        expressionEvaluator.Evaluate($"obj.{PropertyName} = value");
                    }
                }
                catch
                { }
            }
        }
    }
}
