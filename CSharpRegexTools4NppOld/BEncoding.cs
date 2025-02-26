using CSharpRegexTools4Npp.PluginInfrastructure;
using System.Text;

namespace CSharpRegexTools4Npp
{
    /// <summary>
    /// Offre des fonctions simples pour convertir l'encodage d'un texte
    /// entre l'encodage du document courant dans Notepad++ et l'encodage en C# (UTF8)
    /// </summary>
    internal static class BEncoding
    {
        private static readonly Encoding utf8 = Encoding.UTF8;

        /// <summary>
        /// Convertit le texte spécifier de l'encodage du document Notepad++ courant à l'encodage C# (UTF8)
        /// </summary>
        public static string GetUtf8TextFromScintillaText(string scText)
        {
            switch ((int)Win32.SendMessage(PluginBase.nppData._nppHandle, SciMsg.SCI_GETCODEPAGE, 0, 0))
            {
                case 65001: // UTF8
                    return utf8.GetString(Encoding.Default.GetBytes(scText));
                default:
                    Encoding ANSI = Encoding.GetEncoding(1252);

                    byte[] ansiBytes = ANSI.GetBytes(scText);
                    byte[] utf8Bytes = Encoding.Convert(ANSI, Encoding.UTF8, ansiBytes);

                    return Encoding.UTF8.GetString(utf8Bytes);
            }
        }

        /// <summary>
        /// Convertit le texte spécifier de l'encodage C# (UTF8) à l'encodage document Notepad++ courant
        /// </summary>
        public static string GetScintillaTextFromUtf8Text(string utf8Text)
        {
            switch ((int)Win32.SendMessage(PluginBase.nppData._nppHandle, SciMsg.SCI_GETCODEPAGE, 0, 0))
            {
                case 65001: // UTF8
                    return Encoding.Default.GetString(utf8.GetBytes(utf8Text));
                default:
                    Encoding ANSI = Encoding.GetEncoding(1252);

                    byte[] utf8Bytes = utf8.GetBytes(utf8Text);
                    byte[] ansiBytes = Encoding.Convert(Encoding.UTF8, ANSI, utf8Bytes);

                    return ANSI.GetString(ansiBytes);
            }
        }

        /// <summary>
        /// Convertit le texte spécifier de l'encodage C# (UTF8) à l'encodage document Notepad++ courant
        /// </summary>
        public static string GetScintillaTextFromUtf8Text(string utf8Text, out int length)
        {
            byte[] utf8Bytes = utf8.GetBytes(utf8Text);
            length = utf8Bytes.Length;

            switch ((int)Win32.SendMessage(PluginBase.nppData._nppHandle, SciMsg.SCI_GETCODEPAGE, 0, 0))
            {
                case 65001: // UTF8
                    return Encoding.Default.GetString(utf8.GetBytes(utf8Text));
                default:
                    Encoding ANSI = Encoding.GetEncoding(1252);
                    byte[] ansiBytes = Encoding.Convert(Encoding.UTF8, ANSI, utf8Bytes);
                    return ANSI.GetString(ansiBytes);
            }
        }
    }
}
