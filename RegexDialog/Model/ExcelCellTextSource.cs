using ClosedXML.Excel;
using Newtonsoft.Json;
using System;

namespace RegexDialog
{
    public class ExcelCellTextSource : NotifyPropertyChangedBaseClass
    {
        public bool IsSelected { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public Func<IXLCell, string> GetValue { get; set; }
    }
}