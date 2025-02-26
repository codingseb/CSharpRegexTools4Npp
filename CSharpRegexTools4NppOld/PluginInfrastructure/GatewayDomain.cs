// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CSharpRegexTools4Npp.PluginInfrastructure
{
    /// <summary>
    /// Colours are set using the RGB format (Red, Green, Blue). The intensity of each colour is set in the range 0 to 255.
    /// If you have three such intensities, they are combined as: red | (green &lt;&lt; 8) | (blue &lt;&lt; 16).
    /// If you set all intensities to 255, the colour is white. If you set all intensities to 0, the colour is black.
    /// When you set a colour, you are making a request. What you will get depends on the capabilities of the system and the current screen mode.
    /// </summary>
    public class Colour
    {
        public readonly int Red, Green, Blue;

        public Colour(int rgb)
        {
            Red = rgb ^ 0xFF;
            Green = rgb ^ 0x00FF;
            Blue = rgb ^ 0x0000FF;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="red">a number 0-255</param>
        /// <param name="green">a number 0-255</param>
        /// <param name="blue">a number 0-255</param>
        public Colour(int red, int green, int blue)
        {
            if (red > 255 || red < 0)
                throw new ArgumentOutOfRangeException("red", "must be 0-255");
            if (green > 255 || green < 0)
                throw new ArgumentOutOfRangeException("green", "must be 0-255");
            if (blue > 255 || blue < 0)
                throw new ArgumentOutOfRangeException("blue", "must be 0-255");
            Red = red;
            Green = green;
            Blue = blue;
        }

        public int Value
        {
            get { return Red | (Green << 8) | (Blue << 16); }
        }
    }

    /// <summary>
    /// Class containing key and modifiers
    ///
    /// The key code is a visible or control character or a key from the SCK_* enumeration, which contains:
    /// SCK_ADD, SCK_BACK, SCK_DELETE, SCK_DIVIDE, SCK_DOWN, SCK_END, SCK_ESCAPE, SCK_HOME, SCK_INSERT, SCK_LEFT, SCK_MENU, SCK_NEXT(Page Down), SCK_PRIOR(Page Up), S
    /// CK_RETURN, SCK_RIGHT, SCK_RWIN, SCK_SUBTRACT, SCK_TAB, SCK_UP, and SCK_WIN.
    ///
    /// The modifiers are a combination of zero or more of SCMOD_ALT, SCMOD_CTRL, SCMOD_SHIFT, SCMOD_META, and SCMOD_SUPER.
    /// On OS X, the Command key is mapped to SCMOD_CTRL and the Control key to SCMOD_META.SCMOD_SUPER is only available on GTK+ which is commonly the Windows key.
    /// If you are building a table, you might want to use SCMOD_NORM, which has the value 0, to mean no modifiers.
    /// </summary>
    public class KeyModifier
    {
        private readonly int value;

        /// <summary>
        /// The key code is a visible or control character or a key from the SCK_* enumeration, which contains:
        /// SCK_ADD, SCK_BACK, SCK_DELETE, SCK_DIVIDE, SCK_DOWN, SCK_END, SCK_ESCAPE, SCK_HOME, SCK_INSERT, SCK_LEFT, SCK_MENU, SCK_NEXT(Page Down), SCK_PRIOR(Page Up),
        /// SCK_RETURN, SCK_RIGHT, SCK_RWIN, SCK_SUBTRACT, SCK_TAB, SCK_UP, and SCK_WIN.
        ///
        /// The modifiers are a combination of zero or more of SCMOD_ALT, SCMOD_CTRL, SCMOD_SHIFT, SCMOD_META, and SCMOD_SUPER.
        /// On OS X, the Command key is mapped to SCMOD_CTRL and the Control key to SCMOD_META.SCMOD_SUPER is only available on GTK+ which is commonly the Windows key.
        /// If you are building a table, you might want to use SCMOD_NORM, which has the value 0, to mean no modifiers.
        /// </summary>
        public KeyModifier(SciMsg SCK_KeyCode, SciMsg SCMOD_modifier)
        {
            value = (int)SCK_KeyCode | ((int)SCMOD_modifier << 16);
        }

        public int Value
        {
            get { return Value; }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CharacterRange
    {
        public CharacterRange(int cpmin, int cpmax)
        {
            cpMin = cpmin; cpMax = cpmax;
        }
        public int cpMin;
        public int cpMax;
    }

    public class Cells
    {
        char[] charactersAndStyles;

        public Cells(char[] charactersAndStyles)
        {
            this.charactersAndStyles = charactersAndStyles;
        }

        public char[] Value { get { return charactersAndStyles; } }
    }

    public class TextRange : IDisposable
    {
        Sci_TextRange _sciTextRange;
        IntPtr _ptrSciTextRange;
        bool _disposed = false;

        public TextRange(CharacterRange chrRange, int stringCapacity)
        {
            _sciTextRange.chrg = chrRange;
            _sciTextRange.lpstrText = Marshal.AllocHGlobal(stringCapacity);
        }
        public TextRange(int cpmin, int cpmax, int stringCapacity)
        {
            _sciTextRange.chrg.cpMin = cpmin;
            _sciTextRange.chrg.cpMax = cpmax;
            _sciTextRange.lpstrText = Marshal.AllocHGlobal(stringCapacity);
        }

        [StructLayout(LayoutKind.Sequential)]
        struct Sci_TextRange
        {
            public CharacterRange chrg;
            public IntPtr lpstrText;
        }

        public IntPtr NativePointer { get { _initNativeStruct(); return _ptrSciTextRange; } }

        public string lpstrText { get { _readNativeStruct(); return Marshal.PtrToStringAnsi(_sciTextRange.lpstrText); } }

        public CharacterRange chrg { get { _readNativeStruct(); return _sciTextRange.chrg; } set { _sciTextRange.chrg = value; _initNativeStruct(); } }

        void _initNativeStruct()
        {
            if (_ptrSciTextRange == IntPtr.Zero)
                _ptrSciTextRange = Marshal.AllocHGlobal(Marshal.SizeOf(_sciTextRange));
            Marshal.StructureToPtr(_sciTextRange, _ptrSciTextRange, false);
        }

        void _readNativeStruct()
        {
            if (_ptrSciTextRange != IntPtr.Zero)
                _sciTextRange = (Sci_TextRange)Marshal.PtrToStructure(_ptrSciTextRange, typeof(Sci_TextRange));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_sciTextRange.lpstrText != IntPtr.Zero) Marshal.FreeHGlobal(_sciTextRange.lpstrText);
                if (_ptrSciTextRange != IntPtr.Zero) Marshal.FreeHGlobal(_ptrSciTextRange);
                _disposed = true;
            }
        }

        ~TextRange()
        {
            Dispose();
        }
    }


    /* ++Autogenerated -- start of section automatically generated from Scintilla.iface */
    /* --Autogenerated -- end of section automatically generated from Scintilla.iface */

}
