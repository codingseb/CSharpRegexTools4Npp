using System.Collections.Generic;

namespace RegexDialog
{
    public class ExcelCellFilterExample
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public ExcelCellFilterExample(string name, string value)
        {
            Name=name;
            Value=value;
        }

        public static List<ExcelCellFilterExample> ExamplesList { get; set; } = new()
        {
            new("Only one cell", "C9"),
            new("Multiple cells","A2;C3;D1;D4;BD10"),
            new("Only the row 5", "5"),
            new("Some contiguous rows", "3:6"),
            new("Some discontiguous rows", "3;6;10:12"),
            new("Only column D", "D"),
            new("Some contiguous columns", "C:F"),
            new("Some discontiguous columns", "B;D;F:H"),
            new("Only the first used row", "{FR}"),
            new("Only the last used row", "{LR}"),
            new("Only the first used column", "{FC}"),
            new("Only the last used column", "{LC}"),
            new("Only the 5th row of used cells", "{FR + 4}"),
            new("All rows without header row", "{FR + 1}:{LR}"),
            new("Half of rows", "{FR}:{(LR-FR)/ 2 + FR}"),
            new("All used cells without FR,LF,FC and LC", "{(FCN + 1).ToLetter()}{FR + 1}:{(LCN - 1).ToLetter()}{LR - 1}"),
        };
    }
}