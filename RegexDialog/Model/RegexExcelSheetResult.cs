using System.Text.RegularExpressions;

namespace RegexDialog
{
    internal class RegexExcelSheetResult :RegexResult
    {
        public RegexExcelSheetResult(Regex regex, Capture regexElement, int regexElementNb, string fileName, string sheetName) : base(regex, regexElement, regexElementNb, fileName, 0)
        {
            SheetName = sheetName;
        }
        public override string Name => $"Sheet [{SheetName}]: {Children.Count} matches found";

        public string SheetName { get; set; }

        public override bool IsExpanded { get => true; }

        public override void RefreshExpands()
        {
            Children.ForEach(child => child.RefreshExpands());
        }

        public override string ElementType => "Excel";
    }
}