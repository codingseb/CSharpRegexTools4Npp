using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace RegexDialog
{
    internal static class Extensions
    {
        public static string EscapeXml(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            return !SecurityElement.IsValidText(s) ? SecurityElement.Escape(s) : s;
        }

        public static string UnescapeXml(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            string returnString = s;

            returnString = returnString.Replace("&apos;", "'");
            returnString = returnString.Replace("&quot;", "\"");
            returnString = returnString.Replace("&gt;", ">");
            returnString = returnString.Replace("&lt;", "<");
            returnString = returnString.Replace("&amp;", "&");

            return returnString;
        }

        public static string RegexReplace(this string input, string pattern, string replacement, RegexOptions options = RegexOptions.None)
        {
            return Regex.Replace(input, pattern, replacement, options);
        }

        public static string RegexReplace(this string input, string pattern, MatchEvaluator evaluator, RegexOptions options = RegexOptions.None)
        {
            return Regex.Replace(input, pattern, evaluator, options);
        }

        public static string ToLiteral(this string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, new CodeGeneratorOptions { IndentString = "\t" });
                    var literal = writer.ToString();
                    return literal.Replace(string.Format("\" +{0}\t\"", Environment.NewLine), "").Trim('"');
                }
            }
        }
    }
}
