using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace RegexDialog.Converters
{
    /// <summary>
    /// Converter that use a string mathematical or pseudo C# expression to make the conversion.
    /// Use <c>binding</c> to inject the binding value in the expression (example <c>Abs(binding) + 1</c>)
    /// </summary>
    [ContentProperty("Expression")]
    public class ExpressionEvalConverter : BaseConverter, IValueConverter
    {
        /// <summary>
        /// The expression to evaluate to make the conversion. Use <c>binding</c> to inject the binding value in the expression. By default just <c>binding</c>
        /// </summary>
        public string Expression { get; set; } = "binding";

        /// <summary>
        /// The expression to evaluate to make the back conversion. Use <c>binding</c> to inject the binding value in the expression. By default just <c>binding</c>
        /// </summary>
        public string ExpressionForConvertBack { get; set; } = "binding";

        /// <summary>
        /// If <c>true</c> evaluate a string binding as an expression, if false just inject the binding in the Expression, By default : <c>false</c>
        /// </summary>
        public bool EvaluateBindingAsAnExpression { get; set; }

        /// <summary>
        /// If <c>true</c> evaluate a string binding as an expression, if <c>false</c> just inject the binding in the ExpressionForConvertBack, By default : <c>false</c>
        /// </summary>
        public bool EvaluateBindingAsAnExpressionForConvertBack { get; set; }

        /// <summary>
        /// If <c>true</c> Evaluate function is callables in an expression. If <c>false</c> Evaluate is not callable.
        /// By default : false for security
        /// </summary>
        public bool IsEvaluateFunctionActivated { get; set; }

        /// <summary>
        /// If <c>true</c> throw up all evaluate exceptions, if <c>false</c> just return the exception message as a string, By default <c>false</c>
        /// </summary>
        public bool ThrowExceptions { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                ExpressionEvaluator evaluator = new ExpressionEvaluator();

                evaluator.Namespaces.NamespacesListForWPFConverters();

                evaluator.OptionEvaluateFunctionActive = IsEvaluateFunctionActivated;

                if (EvaluateBindingAsAnExpression)
                {
                    evaluator.Variables["binding"] = evaluator.Evaluate(value.ToString());
                }
                else
                {
                    evaluator.Variables["binding"] = value;
                }

                return evaluator.Evaluate(Expression);
            }
            catch (Exception ex)
            {
                if (ThrowExceptions)
                {
                    throw;
                }
                else
                {
                    return ex.Message;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                ExpressionEvaluator evaluator = new ExpressionEvaluator();

                evaluator.Namespaces.NamespacesListForWPFConverters();

                evaluator.OptionEvaluateFunctionActive = IsEvaluateFunctionActivated;

                if (EvaluateBindingAsAnExpressionForConvertBack)
                {
                    evaluator.Variables["binding"] = evaluator.Evaluate(value.ToString());
                }
                else
                {
                    evaluator.Variables["binding"] = value;
                }

                return evaluator.Evaluate(ExpressionForConvertBack);
            }
            catch (Exception ex)
            {
                if (ThrowExceptions)
                {
                    throw;
                }
                else
                {
                    return ex.Message;
                }
            }
        }
    }
}
