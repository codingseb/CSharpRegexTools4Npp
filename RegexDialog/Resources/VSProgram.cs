using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace projectname
{
    class Program
    {
        private static string pattern = "$pattern$";
        private static string replacement = "$replacement$";
        private static RegexOptions options = _options_;
        private static Regex regex = new Regex(pattern, options);

        private static string input = (new TextSourceContainer()).Get().ToString();

        static void Main(string[] args)
        {
            // To make a replace
            string replace = regex.Replace(input, replacement);
            Console.WriteLine(replace);

            //To get all matches
            MatchCollection matches = regex.Matches(input);
            Console.WriteLine(string.Join("\r\n", matches.Cast<Match>().Select(match => match.Value)));
        }
    }
}
