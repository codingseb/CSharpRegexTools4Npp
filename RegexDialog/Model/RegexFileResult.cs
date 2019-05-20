using System.Text.RegularExpressions;

namespace RegexDialog
{
    internal class RegexFileResult : RegexResult
    {
        public RegexFileResult(Regex regex, Capture regexElement, int regexElementNb, string fileName) : base(regex, regexElement, regexElementNb, fileName, 0)
        {}

        public override string Name => $"File {RegexElementNb}: {Children.Count} matches found in \"{FileName}\"";

        public override bool IsExpanded { get => true; }

        public override void RefreshExpands()
        {
            Children.ForEach(child => child.RefreshExpands());
        }

        public override string ElementType => "File";
    }
}
