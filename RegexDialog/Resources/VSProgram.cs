using System;
using System.Text;
using System.Text.RegularExpressions;

namespace projectname
{
    class Program
    {
        private static string pattern = "$pattern$";
        private static string replacement = "$replacement$";
        private static Regex regex = new Regex(pattern);

        private static string input = (new TextSourceContainer()).Get().ToString();

        static void Main(string[] args)
        {
            // To make a replace
            string replace = regex.Replace(input, replacement);
            Console.WriteLine(replace);

        }
    }
}
