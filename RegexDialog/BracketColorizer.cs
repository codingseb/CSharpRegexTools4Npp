using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace RegexDialog
{
    internal class BracketColorizer : DocumentColorizingTransformer
    {
        public int StartOffset { get; set; }
        public int EndOffset { get; set; }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (line.Length > 0 && StartOffset < EndOffset)
            {
                if (StartOffset >= line.Offset && EndOffset <= line.Offset + line.Length)
                {
                    ChangeLinePart(StartOffset, EndOffset, delegate (VisualLineElement element)
                    {
                        element.TextRunProperties.SetBackgroundBrush(Brushes.LightGray);
                    });
                }
            }
        }

    }
}
