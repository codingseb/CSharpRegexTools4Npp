using System;
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
    }
}
