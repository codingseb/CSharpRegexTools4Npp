using System.Collections.Generic;

namespace RegexDialog
{
    /// <summary>
    /// Contains a list of namespaces to add to and to remove from the sub ExpressionEvaluator of ExpressionEvalConverters
    /// </summary>
    internal static class NamespacesForExpressionEvalConverters
    {
        public static List<string> NamespaceToAdd { get; } =
        [
            "System.Windows",
            "System.Windows.Controls",
            "System.Windows.Media",
            "System.Windows.Shapes",
        ];
        public static List<string> NamespaceToRemove { get; } =
        [
            "System.IO",
        ];

        public static void NamespacesListForWPFConverters(this IList<string> list)
        {
            ((List<string>)list).RemoveAll(ns => NamespaceToRemove.Contains(ns));
            ((List<string>)list).AddRange(NamespaceToAdd);
        }
    }
}
