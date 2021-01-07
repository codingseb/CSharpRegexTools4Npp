using System.Collections.Generic;
using System.Diagnostics;

namespace RegexDialog
{
    [DebuggerDisplay("[{GetType().Name}] - {Name} - {Description}")]
    internal class RegexLanguageElementGroup : NotifyPropertyChangedBaseClass
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<RegexLanguageElement> Elements { get; set; }

        public bool IsExpanded { get; set; }

        public bool Visible { get; set; } = true;
    }
}
