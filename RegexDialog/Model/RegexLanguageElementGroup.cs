using System.Collections.Generic;

namespace RegexDialog
{
    internal class RegexLanguageElementGroup
    {
        public string Name
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public List<RegexLanguageElement> Elements
        {
            get;
            set;
        }
    }
}
