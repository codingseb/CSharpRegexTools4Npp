using System.Security;

namespace RegexDialog
{
    internal static class XmlStringExtension
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
