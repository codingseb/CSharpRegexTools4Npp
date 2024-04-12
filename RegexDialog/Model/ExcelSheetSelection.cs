using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RegexDialog
{
    public class ExcelSheetSelection : NotifyPropertyChangedBaseClass
    {
        private readonly static Regex simpleCellRegex = new Regex("^[A-Z]+[1-9][0-9]*$", RegexOptions.Compiled);
        private readonly static Regex simpleColumnRegex = new Regex("^[A-Z]+$", RegexOptions.Compiled);
        private readonly static Regex simpleRowRegex = new Regex("^[1-9][0-9]*$", RegexOptions.Compiled);
        private readonly static Regex rangeRegex = new Regex("^[A-Z]+([1-9][0-9]*)?:[A-Z]+([1-9][0-9]*)?|[1-9][0-9]*:[1-9][0-9]*$", RegexOptions.Compiled);
        private readonly static Regex interpretedStuffRegex = new Regex(@"\[(?<var>FR|LR|FC|LC)\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public bool IsSelected { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public string Filter { get; set; } = string.Empty;

        public IEnumerable<IXLCell> GetCells(IXLWorksheet sheet)
        {
            if (string.IsNullOrWhiteSpace(Filter))
            {
                foreach (var item in sheet.CellsUsed())
                {
                    yield return item;
                }
            }

            foreach (string filter in InterpretStuffInFilter(Filter, sheet).Split(';').Select(filter => filter.Trim().ToUpper()))
            {
                if (simpleCellRegex.IsMatch(filter))
                    yield return sheet.Cell(filter);
                if (simpleColumnRegex.IsMatch(filter))
                {
                    foreach (var cell in sheet.Column(filter).CellsUsed())
                    {
                        yield return cell;
                    }
                }
                else if (simpleRowRegex.IsMatch(filter))
                {
                    foreach (var cell in sheet.Row(int.Parse(filter)).CellsUsed())
                    {
                        yield return cell;
                    }
                }
                else if (rangeRegex.IsMatch(filter))
                {
                    foreach (var cell in sheet.Range(filter).CellsUsed())
                    {
                        yield return cell;
                    }
                }
            }
        }

        private string InterpretStuffInFilter(string filter, IXLWorksheet sheet)
        {
            return interpretedStuffRegex.Replace(filter, match =>
            {
                switch(match.Groups["var"].Value)
                {
                    case "FR":
                        return sheet.FirstRowUsed().RangeAddress.FirstAddress.RowNumber.ToString();
                    case "LR":
                        return sheet.LastRowUsed().RangeAddress.FirstAddress.RowNumber.ToString();
                    case "FC":
                        return sheet.FirstColumnUsed().RangeAddress.FirstAddress.ColumnLetter;
                    case "LC":
                        return sheet.LastColumnUsed().RangeAddress.FirstAddress.ColumnLetter;
                    default:
                        return "";
                }
            });
        }
    }
}