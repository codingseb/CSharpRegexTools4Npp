using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace _projectname_
{
    class Program
    {
        private readonly static string pattern = "$pattern$";
        private readonly static string replacement = "$replacement$";
        private readonly static RegexOptions options = _options_;
        private readonly static Regex regex = new Regex(pattern, options);

        private static string input = (new TextSourceContainer()).Get().ToString();

        static void Main(string[] args)
        {
//code
        }
    }
}
