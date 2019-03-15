﻿using CSharpRegexTools4Npp.PluginInfrastructure;
using System;
using System.Text;

namespace CSharpRegexTools4Npp
{
    public class BNpp
    {
        public static NotepadPPGateway NotepadPP { get; private set; } = new NotepadPPGateway();

        /// <summary>
        /// Récupère les caractères de fin de lignes courant
        /// !!! Attention pour le moment bug. !!! Enlève la coloration syntaxique du fichier courant
        /// </summary>
        public static string CurrentEOL
        {
            get
            {
                string eol = "\n";
                int value = Win32.SendMessage(PluginBase.nppData._nppHandle, SciMsg.SCI_GETEOLMODE, 0, 0).ToInt32();

                switch (value)
                {
                    case 0:
                        eol = "\r\n";
                        break;
                    case 1:
                        eol = "\r";
                        break;
                    default:
                        break;
                }

                return eol;
            }
        }

        /// <summary>
        /// Récupère ou attribue le texte complet du tab Notepad++ courant
        /// <br/>(Gère la conversion d'encodage Npp/C#)
        /// </summary>
        public static string Text
        {
            get
            {
                IScintillaGateway scintilla = new ScintillaGateway(PluginBase.GetCurrentScintilla());
                // Multiply by 2 to managed 2 bytes encoded chars
                return BEncoding.GetUtf8TextFromScintillaText(scintilla.GetText(scintilla.GetTextLength() * 2));
            }

            set
            {
                IScintillaGateway scintilla = new ScintillaGateway(PluginBase.GetCurrentScintilla());
                string text = BEncoding.GetScintillaTextFromUtf8Text(value, out int length);
                scintilla.SetText(text);
            }
        }

        /// <summary>
        /// Récupère ou attribue le début de la sélection de texte
        /// <br/>(Gère la conversion d'encodage Npp/C#)
        /// </summary>
        public static int SelectionStart
        {
            get
            {
                return new ScintillaGateway(PluginBase.GetCurrentScintilla()).GetSelectionStart();
            }

            set
            {
                new ScintillaGateway(PluginBase.GetCurrentScintilla()).SetSelectionStart(new Position(value));
            }
        }

        /// <summary>
        /// Récupère ou attribue la fin de la sélection de texte
        /// <br/>si aucun texte n'est sélectionné SelectionEnd = SelectionStart
        /// <br/>(Gère la conversion d'encodage Npp/C#)
        /// </summary>
        public static int SelectionEnd
        {
            get
            {
                int curPos = (int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GETSELECTIONEND, 0, 0);
                IScintillaGateway scintilla = new ScintillaGateway(PluginBase.GetCurrentScintilla());
                string beginingText = scintilla.GetText(curPos);
                string text = BEncoding.GetScintillaTextFromUtf8Text(beginingText, out int length);
                return length;
            }

            set
            {
                string allText = Text;
                int endToUse = value;

                if (value < 0)
                {
                    endToUse = 0;
                }
                else if (value > allText.Length)
                {
                    endToUse = allText.Length;
                }

                string afterText = allText.Substring(0, endToUse);
                string afterTextInDefaultEncoding = BEncoding.GetScintillaTextFromUtf8Text(afterText, out int defaultEnd);

                Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_SETSELECTIONEND, defaultEnd, 0);
            }
        }

        /// <summary>
        /// Récupère ou attribue la longueur de la sélection de texte
        /// <br/>Si aucun texte n'est sélectionné SelectionEnd = 0
        /// <br/>(Gère la conversion d'encodage Npp/C#)
        /// </summary>
        public static int SelectionLength
        {
            get
            {
                return SelectionEnd - SelectionStart;
            }

            set
            {
                SelectionEnd = SelectionStart + (value < 0 ? 0 : value);
            }
        }

        /// <summary>
        /// Récupère ou remplace le texte actuellement sélectionné
        /// <br/>(Gère la conversion d'encodage Npp/C#)
        /// </summary>
        public static string SelectedText
        {
            get
            {
                IScintillaGateway scintillaGateway = new ScintillaGateway(PluginBase.GetCurrentScintilla());
                int start = scintillaGateway.GetSelectionStart().Value;
                int end = scintillaGateway.GetSelectionEnd().Value;

                return end - start == 0 ? "" : Text.Substring(start, end - start);
            }

            set
            {
                string defaultNewText = BEncoding.GetScintillaTextFromUtf8Text(value);
                Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_REPLACESEL, 0, defaultNewText);
            }
        }

        /// <summary>
        /// Sélectionne dans le tab Notepad++ courant le texte entre start et end
        /// et positionne le scroll pour voir la sélection.
        /// <br/>(Gère la conversion d'encodage Npp/C#)
        /// </summary>
        /// <param name="start">Position du début du texte à sélectionner dans le texte entier<br/> Si plus petit que 0 -> forcé à zéro<br/> Si plus grand que Text.Length -> forcé à Text.Length</param>
        /// <param name="end">Position de fin du texte à sélectionner dans le texte entier<br/> Si plus petit que 0 -> forcé à zéro<br/> Si plus grand que Text.Length -> forcé à Text.Length<br/> Si plus petit que start -> forcé à start</param>
        public static void SelectTextAndShow(int start, int end)
        {
            string allText = Text;
            int startToUse = start;
            int endToUse = end;

            if (start < 0)
            {
                startToUse = 0;
            }
            else if (start > allText.Length)
            {
                startToUse = allText.Length;
            }

            if (end < 0)
            {
                endToUse = 0;
            }
            else if (end > allText.Length)
            {
                endToUse = allText.Length;
            }
            else if (endToUse < startToUse)
            {
                endToUse = startToUse;
            }

            string beforeText = allText.Substring(0, startToUse);
            string beforeTextInDefaultEncoding = BEncoding.GetScintillaTextFromUtf8Text(beforeText, out int defaultStart);
            string endText = allText.Substring(0, endToUse);
            string endTextInDefaultEncoding = BEncoding.GetScintillaTextFromUtf8Text(endText, out int defaultEnd);

            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GOTOPOS, defaultStart, 0);
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_SETSELECTIONEND, defaultEnd, 0);
        }

        /// <summary>
        /// Si la sélection multiple est activée ajoute la sélection spécifié
        /// <br/>(Gère la conversion d'encodage Npp/C#)
        /// </summary>
        /// <param name="start">Position du début du texte à sélectionner dans le texte entier<br/> Si plus petit que 0 -> forcé à zéro<br/> Si plus grand que Text.Length -> forcé à Text.Length</param>
        /// <param name="end">Position de fin du texte à sélectionner dans le texte entier<br/> Si plus petit que 0 -> forcé à zéro<br/> Si plus grand que Text.Length -> forcé à Text.Length<br/> Si plus petit que start -> forcé à start</param>

        public static void AddSelection(int start, int end)
        {
            string allText = Text;
            int startToUse = start;
            int endToUse = end;

            if (start < 0)
            {
                startToUse = 0;
            }
            else if (start > allText.Length)
            {
                startToUse = allText.Length;
            }

            if (end < 0)
            {
                endToUse = 0;
            }
            else if (end > allText.Length)
            {
                endToUse = allText.Length;
            }
            else if (endToUse < startToUse)
            {
                endToUse = startToUse;
            }

            string beforeText = allText.Substring(0, startToUse);
            string beforeTextInDefaultEncoding = BEncoding.GetScintillaTextFromUtf8Text(beforeText, out int defaultStart);
            string endText = allText.Substring(0, endToUse);
            string endTextInDefaultEncoding = BEncoding.GetScintillaTextFromUtf8Text(endText, out int defaultEnd);

            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_ADDSELECTION, defaultStart, defaultEnd);
        }


        /// <summary>
        /// Récupère le texte de la ligne spécifiée
        /// </summary>
        /// <param name="lineNb">Numéro de la ligne dont on veut récupérer le texte</param>
        /// <returns>Le texte de la ligne spécifiée</returns>
        public static string GetLineText(int lineNb)
        {
            string result = "";

            try
            {
                result = Text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)[lineNb - 1];
            }
            catch { }

            return result;
        }
    }

    /// <summary>
    /// Offre des fonctions simples pour convertir l'encodage d'un texte
    /// entre l'encodage du document courant dans Notepad++ et l'encodage en C# (UTF8)
    /// </summary>
    internal static class BEncoding
    {
        private static Encoding utf8 = Encoding.UTF8;

        /// <summary>
        /// Convertit le texte spécifier de l'encodage du document Notepad++ courant à l'encodage C# (UTF8)
        /// </summary>
        public static string GetUtf8TextFromScintillaText(string scText)
        {
            string result = "";
            int iEncoding = (int)Win32.SendMessage(PluginBase.nppData._nppHandle, SciMsg.SCI_GETCODEPAGE, 0, 0);

            switch (iEncoding)
            {
                case 65001: // UTF8
                    result = utf8.GetString(Encoding.Default.GetBytes(scText));
                    break;
                default:
                    Encoding ANSI = Encoding.GetEncoding(1252);

                    byte[] ansiBytes = ANSI.GetBytes(scText);
                    byte[] utf8Bytes = Encoding.Convert(ANSI, Encoding.UTF8, ansiBytes);

                    result = Encoding.UTF8.GetString(utf8Bytes);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Convertit le texte spécifier de l'encodage C# (UTF8) à l'encodage document Notepad++ courant
        /// </summary>
        public static string GetScintillaTextFromUtf8Text(string utf8Text)
        {
            string result = "";
            int iEncoding = (int)Win32.SendMessage(PluginBase.nppData._nppHandle, SciMsg.SCI_GETCODEPAGE, 0, 0);

            switch (iEncoding)
            {
                case 65001: // UTF8
                    result = Encoding.Default.GetString(utf8.GetBytes(utf8Text));
                    break;
                default:
                    Encoding ANSI = Encoding.GetEncoding(1252);

                    byte[] utf8Bytes = utf8.GetBytes(utf8Text);
                    byte[] ansiBytes = Encoding.Convert(Encoding.UTF8, ANSI, utf8Bytes);

                    result = ANSI.GetString(ansiBytes);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Convertit le texte spécifier de l'encodage C# (UTF8) à l'encodage document Notepad++ courant
        /// </summary>
        public static string GetScintillaTextFromUtf8Text(string utf8Text, out int length)
        {
            string result = "";
            int iEncoding = (int)Win32.SendMessage(PluginBase.nppData._nppHandle, SciMsg.SCI_GETCODEPAGE, 0, 0);

            byte[] utf8Bytes = utf8.GetBytes(utf8Text);
            length = utf8Bytes.Length;

            switch (iEncoding)
            {
                case 65001: // UTF8
                    result = Encoding.Default.GetString(utf8.GetBytes(utf8Text));
                    break;
                default:
                    Encoding ANSI = Encoding.GetEncoding(1252);
                    byte[] ansiBytes = Encoding.Convert(Encoding.UTF8, ANSI, utf8Bytes);
                    result = ANSI.GetString(ansiBytes);
                    break;
            }

            return result;
        }
    }
}
