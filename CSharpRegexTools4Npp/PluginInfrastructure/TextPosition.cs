﻿// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.
using System;

namespace CSharpRegexTools4Npp.PluginInfrastructure
{
    /// <summary>
    /// Positions within the Scintilla document refer to a character or the gap before that character.
    /// The first character in a document is 0, the second 1 and so on. If a document contains nLen characters, the last character is numbered nLen-1. The caret exists between character positions and can be located from before the first character (0) to after the last character (nLen).
    ///
    /// There are places where the caret can not go where two character bytes make up one character.
    /// This occurs when a DBCS character from a language like Japanese is included in the document or when line ends are marked with the CP/M
    /// standard of a carriage return followed by a line feed.The INVALID_POSITION constant(-1) represents an invalid position within the document.
    ///
    /// All lines of text in Scintilla are the same height, and this height is calculated from the largest font in any current style.This restriction
    /// is for performance; if lines differed in height then calculations involving positioning of text would require the text to be styled first.
    ///
    /// If you use messages, there is nothing to stop you setting a position that is in the middle of a CRLF pair, or in the middle of a 2 byte character.
    /// However, keyboard commands will not move the caret into such positions.
    /// </summary>
    public class TextPosition : IEquatable<TextPosition>
    {
        private readonly int pos;

        public static TextPosition From(int pos)
        {
            return new TextPosition(pos);
        }

        public TextPosition(int pos)
        {
            this.pos = pos;
        }

        public int Value
        {
            get { return pos; }
        }

        public static TextPosition operator +(TextPosition a, TextPosition b)
        {
            return new TextPosition(a.pos + b.pos);
        }

        public static TextPosition operator -(TextPosition a, TextPosition b)
        {
            return new TextPosition(a.pos - b.pos);
        }

        public static bool operator ==(TextPosition a, TextPosition b)
        {
	        if (ReferenceEquals(a, b))
		        return true;
			if (ReferenceEquals(a, null))
				return false;
			if (ReferenceEquals(b, null))
				return false;
			return  a.pos == b.pos;
        }

        public static bool operator !=(TextPosition a, TextPosition b)
        {
            return !(a == b);
        }

        public static bool operator >(TextPosition a, TextPosition b)
        {
            return a.Value > b.Value;
        }

        public static bool operator <(TextPosition a, TextPosition b)
        {
            return a.Value < b.Value;
        }

        public static TextPosition Min(TextPosition a, TextPosition b)
        {
            if (a < b)
                return a;
            return b;
        }

		public static TextPosition Max(TextPosition a, TextPosition b)
		{
			if (a > b)
				return a;
			return b;
		}

		public override string ToString()
        {
            return "TextPosition: " + pos;
        }

        public bool Equals(TextPosition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return pos == other.pos;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextPosition)obj);
        }

        public override int GetHashCode()
        {
            return pos.GetHashCode();
        }
    }
    /* --Autogenerated -- end of section automatically generated from Scintilla.iface */

}
