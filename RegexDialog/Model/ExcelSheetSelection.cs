﻿using ClosedXML.Excel;
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
        private readonly static Regex evaluatedExpressionRegex = new Regex(@"\{(?<expression>[^\}]*)\}", RegexOptions.Compiled);

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
            ExpressionEvaluator evaluator = new ExpressionEvaluator(new Dictionary<string, object>()
            {
                { "FR", sheet.FirstRowUsed().RangeAddress.FirstAddress.RowNumber },
                { "LR", sheet.LastRowUsed().RangeAddress.FirstAddress.RowNumber },
                { "FC", sheet.FirstColumnUsed().RangeAddress.FirstAddress.ColumnLetter },
                { "LC", sheet.LastColumnUsed().RangeAddress.FirstAddress.ColumnLetter },
                { "FCN", sheet.FirstColumnUsed().RangeAddress.FirstAddress.ColumnNumber },
                { "LCN", sheet.LastColumnUsed().RangeAddress.FirstAddress.ColumnNumber },
                { "sheet", sheet },
            });

            evaluator.EvaluateFunction += Evaluator_EvaluateFunction;

            string result = evaluatedExpressionRegex.Replace(filter, match => evaluator.Evaluate(match.Groups["expression"].Value).ToString());

            evaluator.EvaluateFunction -= Evaluator_EvaluateFunction;

            return result;
        }

        private void Evaluator_EvaluateFunction(object sender, FunctionEvaluationEventArg e)
        {
            IXLWorksheet sheet = e.Evaluator.Variables["sheet"] as IXLWorksheet;

            if(e.Name.Equals("ToNumber"))
            {
                if(e.This != null)
                {
                    e.Value = sheet.Column((string)e.This).ColumnNumber();
                }
                else
                {
                    e.Value = sheet.Column(e.EvaluateArg<string>(0)).ColumnNumber();
                }
            }
            if(e.Name.Equals("ToLetter"))
            {
                if(e.This != null)
                {
                    e.Value = sheet.Column((int)e.This).ColumnLetter();
                }
                else
                {
                    e.Value = sheet.Column(e.EvaluateArg<int>(0)).ColumnLetter();
                }
            }
        }
    }
}