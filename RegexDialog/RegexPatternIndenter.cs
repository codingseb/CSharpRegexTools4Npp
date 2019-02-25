using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RegexDialog
{
    /// <summary>
    /// Outils permetant de gérer l'indentation et désindentation automatique d'un pattern de regex
    /// </summary>
    public class RegexPatternIndenter
    {
        /// <summary>
        /// Indent le pattern de regex spécifié
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string IndentRegexPattern(string pattern, bool charClassesOnOneLine, bool keepQuantifierOnSameLine)
        {
            Regex groupComplement = new Regex(@"^[?](([<][=!])|[:=>]|([\-]?[imnsx][:])|([<][a-zA-Z][a-zA-Z0-9_]*[>]))");
            Regex quantifiers = new Regex(@"^(([?+*][?]?)|([{]\d+([,])?)[}])");

            int level = 0;

            string result = "";
            bool isEscapted = false;

            int i = 0;

            string oneLinePattern = SetOnOneLine(pattern);

            while (i < oneLinePattern.Length)
            {
                char character = oneLinePattern[i];
                bool ok = false;

                if (isEscapted)
                {
                    isEscapted = false;
                }
                else
                {
                    if (character == '\\')
                        isEscapted = true;
                    else if (character == '(')
                    {
                        ok = true;

                        if (i > 0)
                        {
                            char lastchar = result.TrimEnd('\t').ToCharArray().Last();

                            if (lastchar != '\r' && lastchar != '\n')
                            {
                                result += "\r\n" + GetTab(level);
                            }
                        }

                        result += character;

                        Match match = groupComplement.Match(oneLinePattern.Substring(i + 1));

                        if (match.Success)
                        {
                            result += match.Value;
                            i += match.Length;
                        }

                        level++;

                        result += "\r\n" + GetTab(level);
                    }
                    else if (character == ')' && level > 0)
                    {
                        level--;

                        result = result.TrimEnd('\t', ' ', '\r', '\n');

                        result += "\r\n" + GetTab(level);

                        result += character;

                        Match match = quantifiers.Match(oneLinePattern.Substring(i + 1));

                        if (match.Success && keepQuantifierOnSameLine)
                        {
                            result += match.Value;
                            i += match.Length;
                        }

                        result += "\r\n" + GetTab(level);

                        ok = true;
                    }
                    else if (character == '[' && charClassesOnOneLine)
                    {
                        char lastchar = result.TrimEnd('\t', ' ').ToCharArray().Last();

                        if (lastchar != '\r' && lastchar != '\n')
                        {
                            result += "\r\n" + GetTab(level);
                        }

                        result += character;

                        ok = true;
                    }
                    else if (character == ']' && charClassesOnOneLine)
                    {
                        result += character;

                        Match match = quantifiers.Match(oneLinePattern.Substring(i + 1));

                        if (match.Success && keepQuantifierOnSameLine)
                        {
                            result += match.Value;
                            i += match.Length;
                        }

                        result += "\r\n" + GetTab(level);

                        ok = true;
                    }
                }

                if (!ok)
                    result += character.ToString();

                i++;
            }

            return Regex.Replace(result.TrimEnd(' ', '\t', '\n', '\r'), @"\r\n([ \t]*\r\n)+", "\r\n", RegexOptions.Singleline);
        }

        private static string GetTab(int nbr)
        {
            string result = "";

            for (int i = 0; i < nbr; i++)
            {
                result += "\t";
            }

            return result;
        }

        /// <summary>
        /// Passe un pattern de regex multiligne et indenté sur une ligne et compressée
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string SetOnOneLine(string pattern)
        {
            string result = Regex.Replace(pattern, @"(?<!(?<!(?<![\\])([\\]{2})*[\\])\[[^\[\]]*)([\t \r\n]+)", "", RegexOptions.Singleline);

            return result;
        }
    }
}
