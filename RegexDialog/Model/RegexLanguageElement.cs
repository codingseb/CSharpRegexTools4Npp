namespace RegexDialog
{
    internal class RegexLanguageElement : NotifyPropertyChangedBaseClass
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Value { get; set; }

        public bool Visible { get; set; } = true;

        public bool IsExpanded { get; set; }
    }
}
