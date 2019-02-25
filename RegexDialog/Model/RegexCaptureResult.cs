using System.Text.RegularExpressions;

namespace RegexDialog
{
    internal class RegexCaptureResult : RegexResult
    {
        public RegexCaptureResult(Regex regex, Capture capture, int captureNb, string fileName = "", int selectionIndex = 0) : base(regex, capture, captureNb, fileName, selectionIndex)
        {}

        public override string ElementType
        {
            get
            {
                return "Capture";
            }
        }
    }
}
