using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace RegexDialog.Converters
{
    /// <summary>
    /// MultiBinding Converter that use a string mathematical or pseudo C# expression to make the conversion.
    /// Use <c>bindings</c> as an array of object to inject bindings values in the expression (example <c>Abs(bindings[0]) + bindings[1]</c>)
    /// </summary>
    [ContentProperty("Expression")]
    public class ExpressionEvalMultiBindingConverter : BaseConverter, IMultiValueConverter
    {
        /// <summary>
        /// To specify a list of namespaces separated by the ; character to add as usings for the evaluator
        /// </summary>
        public string NamespacesToAdd { get; set; } = string.Empty;

        /// <summary>
        /// The expression to evaluate to make the conversion. Use <c>bindings</c>  as an array of object to inject bindings values in the expression. By default just <c>bindings</c>
        /// </summary>
        public string Expression { get; set; } = "bindings";

        /// <summary>
        /// The expression to evaluate to make the back conversion. Use <c>binding</c> to inject the binding value in the expression. By default just <c>binding</c>
        /// Must return an array of object
        /// </summary>
        public string ExpressionForConvertBack { get; set; } = "binding";

        /// <summary>
        /// If <c>>= 0</c> evaluate the binding at corresponding index as an expression, if  just inject the binding in the Expression, By default : <c>-1</c>
        /// </summary>
        public int EvaluateBindingAtIndexAsAnExpression { get; set; } = -1;

        /// <summary>
        /// If <c>true</c> evaluate a string binding as an expression, if <c>false</c> just inject the binding in the ExpressionForConvertBack, By default : <c>false</c>
        /// </summary>
        public bool EvaluateBindingAsAnExpressionForConvertBack { get; set; }

        /// <summary>
        /// If <c>true</c> Evaluate function is callables in an expression. If <c>false</c> Evaluate is not callable.
        /// By default : false for security
        /// </summary>
        public bool OptionEvaluateFunctionActive { get; set; }

        /// <summary>
        /// if true all evaluation are case sensitives, if false evaluations are case insensitive.
        /// By default = true
        /// </summary>
        public bool OptionCaseSensitiveEvaluationActive { get; set; } = true;

        /// <summary>
        /// If <c>true</c> throw up all evaluate exceptions, if <c>false</c> just return the exception message as a string, By default <c>false</c>
        /// </summary>
        public bool ThrowExceptions { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Dictionary<string, object> variables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            try
            {
                ExpressionEvaluator evaluator = new ExpressionEvaluator()
                {
                    OptionCaseSensitiveEvaluationActive = OptionCaseSensitiveEvaluationActive
                };

                evaluator.Namespaces.NamespacesListForWPFConverters();

                NamespacesToAdd.Split(';').ToList().ForEach(namespaceName =>
                {
                    if (!string.IsNullOrWhiteSpace(namespaceName))
                    {
                        evaluator.Namespaces.Add(namespaceName);
                    }
                });

                evaluator.OptionEvaluateFunctionActive = OptionEvaluateFunctionActive;

                variables["bindings"] = values;

                evaluator.Variables = variables;

                if (EvaluateBindingAtIndexAsAnExpression >= 0)
                {
                    return evaluator.Evaluate(values[EvaluateBindingAtIndexAsAnExpression].ToString());
                }
                else
                {
                    return evaluator.Evaluate(Expression);
                }
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            Dictionary<string, object> variables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            try
            {
                ExpressionEvaluator evaluator = new ExpressionEvaluator()
                {
                    OptionCaseSensitiveEvaluationActive = OptionCaseSensitiveEvaluationActive
                };

                evaluator.Namespaces.NamespacesListForWPFConverters();

                evaluator.OptionEvaluateFunctionActive = OptionEvaluateFunctionActive;

                if (EvaluateBindingAsAnExpressionForConvertBack)
                {
                    variables["binding"] = evaluator.Evaluate(value.ToString());
                }
                else
                {
                    variables["binding"] = value;
                }

                evaluator.Variables = variables;

                return (object[])evaluator.Evaluate(ExpressionForConvertBack);
            }
            catch (Exception ex)
            {
                if (ThrowExceptions)
                {
                    throw;
                }
                else
                {
                    return new object[] { ex.Message };
                }
            }
        }
    }
}
