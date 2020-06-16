using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace RegexDialog
{
    internal class RegexResult : NotifyPropertyChangedBaseClass
    {
        public RegexResult(Regex regex, Capture regexElement, int regexElementNb, string fileName = "", int selectionIndex = 0)
        {
            Regex = regex;
            RegexElement = regexElement;
            RegexElementNb = regexElementNb;
            FileName = fileName;
            SelectionIndex = selectionIndex;
        }

        public virtual void RefreshExpands()
        { }

        public virtual bool IsExpanded { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public virtual string ElementType
        {
            get
            {
                return RegexElement.GetType().Name;
            }
        }

        public string FileName { get; set; } = string.Empty;

        public Capture RegexElement { get; } = null;

        public int RegexElementNb { get; } = 0;

        public RegexResult Parent { get; set; } = null;

        public List<RegexResult> Children { get; set; } = new List<RegexResult>();

        public Regex Regex { get; private set; }

        public virtual string Name
        {
            get
            {
                string result = "";

                try
                {
                    if (RegexElement != null)
                    {
                        result = ElementType + " " + RegexElementNb.ToString() + " [" + RegexElement.Index.ToString() + "," + RegexElement.Length.ToString() + "]: ";
                    }
                }
                catch
                { }

                return result;
            }
        }

        public virtual string Value
        {
            get
            {
                string result = "";

                try
                {
                    if (RegexElement != null)
                    {
                        result = RegexElement.Value;
                    }
                }
                catch
                { }

                return result;
            }
        }

        public virtual string OneLineValue
        {
            get
            {
                return Value.Replace("\r", "").Replace("\n", "");
            }
        }

        public virtual int SelectionIndex { get; set; } = 0;

        public virtual int Index
        {
            get
            {
                return (RegexElement?.Index ?? 0) + SelectionIndex;
            }
        }

        public virtual int Length
        {
            get
            {
                return RegexElement?.Length ?? 0;
            }
        }
    }
}
