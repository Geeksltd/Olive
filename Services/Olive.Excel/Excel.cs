using Olive;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Olive.Export
{
    partial class Exporter<T>
    {
        string GenerateExcelWorksheet()
        {
            var result = new StringBuilder();
            result.AddFormattedLine(@"<Worksheet ss:Name=""{0}"">",
                DocumentName.Remove("/", @"\", "?", "*", ":", "[", "]", "\r", "\n").KeepReplacing("  ", " ").Summarize(MAX_LENGTH_FOR_SUMMARIZE, enforceMaxLength: true).XmlEncode());
            result.AddFormattedLine(@"<Table DefaultColumnWidth=""{0}"">", DefaultColumnWidth);

            result.AppendLine(Columns.Select((h, i) => GenerateColumnTag(h, i + 1)).Trim().ToLinesString());

            result.AppendLine(GenerateHeaderGroupings());

            if (!ExcludeHeader)
                result.AppendLine(GenerateSheetHeaderRow());

            result.AppendLine(GenerateDataRows());

            result.AppendLine(@"</Table>");

            result.AppendLine(GenerateDropDownDataValidation());

            result.AppendLine(GenerateWorksheetSettings());

            result.AppendLine(@"</Worksheet>");

            return result.ToString();
        }

        internal string GenerateColumnTag(Column<T> column, int index)
        {
            if (column.Width == null) return null;

            var r = new StringBuilder();

            r.Append($"<Column ss:Index=\"{index}\"");

            if (column.Width.HasValue)
                r.Append($" ss:Width=\"{column.Width}\"");

            r.Append("/>");

            return r.ToString();
        }

        string GenerateWorksheetSettings()
        {
            var frozenHeaderSetting = "<SplitHorizontal>1</SplitHorizontal><TopRowBottomPane>1</TopRowBottomPane>";
            var frozenFirstColumnSetting = "<SplitVertical>1</SplitVertical><LeftColumnRightPane>1</LeftColumnRightPane>";

            return @"<WorksheetOptions xmlns=""urn:schemas-microsoft-com:office:excel"">
                     <Unsynced/>
                     <Selected/>
                     <FreezePanes/>
                     <FrozenNoSplit/>
                     {0}
                     {1}
                     <ProtectObjects>False</ProtectObjects>
                     <ProtectScenarios>False</ProtectScenarios>
                    </WorksheetOptions>"
                    .FormatWith(
                    frozenHeaderSetting.OnlyWhen(FreezeHeader),
                    frozenFirstColumnSetting.OnlyWhen(FreezeFirstColumn));
        }

        string GenerateHeaderGroupings()
        {
            var groups = GetGroups();

            if (groups.None()) return string.Empty;

            var r = new StringBuilder();

            r.AppendLine(@"<Row>");

            foreach (var g in groups)
                r.AddFormattedLine("<Cell ss:StyleID=\"{0}\" ss:MergeAcross=\"{1}\"><Data ss:Type=\"String\" >{2}</Data></Cell>", g.Style.GetStyleId(), g.Quantity, g.GroupName);

            r.AppendLine("</Row>");

            return r.ToString();
        }

        IEnumerable<ColumnGroup> GetGroups()
        {
            var result = new List<ColumnGroup>();

            if (Columns.All(i => i.GroupName.IsEmpty())) return result; // No grouping has been provided.

            foreach (var column in Columns)
            {
                var previousGroup = result.LastOrDefault(r => r.GroupName == column.GroupName);

                if (previousGroup != null)
                {
                    previousGroup.Quantity++;
                }
                else
                {
                    previousGroup = new ColumnGroup { GroupName = column.GroupName, Quantity = 0, Style = column.GroupingStyle };
                    result.Add(previousGroup);
                }
            }

            return result;
        }

        class ColumnGroup
        {
            internal string GroupName;
            internal int Quantity;
            internal CellStyle Style;
        }

        public static string GenerateExcelXml(params Exporter<T>[] sheets)
        {
            if (sheets == null || sheets.None())
                throw new ArgumentException("No excel sheets specified.");

            if (sheets.GroupBy(s => s.DocumentName).Any(x => x.HasMany()))
                throw new ArgumentException("Sheet names should be unique. At least 2 sheets in the provided list have the same DocumentName.");

            var r = new StringBuilder();

            r.AppendLine(@"<?xml version=""1.0""?><?mso-application progid=""Excel.Sheet""?>");
            r.AppendLine(@"<Workbook xmlns=""urn:schemas-microsoft-com:office:spreadsheet""");
            r.AppendLine(@"xmlns:o=""urn:schemas-microsoft-com:office:office""");
            r.AppendLine(@"xmlns:x=""urn:schemas-microsoft-com:office:excel""");
            r.AppendLine(@"xmlns:ss=""urn:schemas-microsoft-com:office:spreadsheet""");
            r.AppendLine(@"xmlns:html=""http://www.w3.org/TR/REC-html40"">");

            // Generate styles
            r.AppendLine(GenerateStyles(sheets));

            // NamedRanges:
            var namedRanges = sheets.SelectMany(s => s.Columns.OfType<DropDownColumn<T>>()).Distinct(c => c.EnumerationName);
            var nameRangeNodes = namedRanges.Select(c => "<NamedRange ss:Name=\"{0}\" ss:RefersTo=\"={0}!R1C1:R{1}C1\"/>".FormatWith(c.EnumerationName, c.PossibleValues.Length));
            r.AddFormattedLine("<Names>{0}</Names>", nameRangeNodes.ToLinesString());

            foreach (var sheet in sheets)
                r.AppendLine(sheet.GenerateExcelWorksheet());

            r.AppendLine(namedRanges.Select(c => GenerateDropDownSourceSheet(c)).ToLinesString());

            r.AppendLine(@"</Workbook>");

            return r.ToString();
        }

        static string GenerateStyles(params Exporter<T>[] sheets)
        {
            var r = new StringBuilder();

            r.AppendLine("<Styles>");

            // Link style
            r.AddFormattedLine(@"<Style ss:ID=""linkStyle"">
                                 <Font ss:Color=""#0000FF"" ss:Underline=""Single""/>
                                 </Style>", sheets.First().HeaderBackGroundColor);

            // Merge settings:
            sheets.Do(s => s.MergeStyles());

            var uniqueStyles = sheets.SelectMany(x => x.GetAllStyles()).Distinct(x => x.GetStyleId()).ToList();
            foreach (var style in uniqueStyles)
                r.AppendLine(style.GenerateStyle());

            r.AppendLine("</Styles>");

            return r.ToString();
        }

        IEnumerable<CellStyle> GetAllStyles()
        {
            var header = Columns.SelectMany(x => new[] { x.HeaderStyle, x.RowStyle });
            var rows = DataRows.SelectMany(x => x.ExceptNull().OfType<Cell>()).Select(x => x.Style);
            var groupings = Columns.Where(c => c.GroupName.HasValue()).Select(x => x.GroupingStyle);

            return header.Concat(rows).Concat(groupings).Distinct(x => x.GetStyleId()).ToArray();
        }

        void MergeStyles()
        {
            foreach (var row in DataRows)
            {
                for (var i = 0; i < row.Length; i++)
                {
                    var cell = row[i] as Cell;

                    if (cell == null) continue;

                    var column = Columns[i];

                    cell.Style = column.RowStyle.OverrideWith(cell.Style);
                }
            }
        }

        /// <summary>
        /// Generates Hidden Worksheets that contain Possible Values for each DropDown
        /// </summary>
        static string GenerateDropDownSourceSheet(DropDownColumn<T> column)
        {
            var rows = column.PossibleValues.
                       Select(v => $@"<Row><Cell><Data ss:Type=""{column.DataType}"">{v}</Data><NamedCell ss:Name=""{column.EnumerationName}""/></Cell></Row>");

            return $@"<Worksheet ss:Name=""{column.EnumerationName}"">
                                    <Table>
                                        {rows.ToLinesString()}
                                    </Table>
                                    <WorksheetOptions xmlns=""urn:schemas-microsoft-com:office:excel"">
                                        <Visible>SheetHidden</Visible>
                                    </WorksheetOptions>
                                </Worksheet>";
        }

        /// <summary>
        /// DataValidation assigns a DropDown for each cell and restrics possible values to that drop down
        /// </summary>
        string GenerateDropDownDataValidation()
        {
            return Columns.OfType<DropDownColumn<T>>().Select(c =>
                   @"<DataValidation xmlns=""urn:schemas-microsoft-com:office:excel"">
                        <Type>List</Type>
                        <Range>R1C{0}:R{1}C{0}</Range>
                        <Value>{2}</Value>
                     </DataValidation>".FormatWith(Columns.IndexOf(c) + 1, DataRows.Count + 1, c.EnumerationName)).ToLinesString();
        }

        string GenerateSheetHeaderRow()
        {
            var r = new StringBuilder();

            r.AppendLine(@"<Row>");

            foreach (var c in Columns)
            {
                r.AppendFormat("<Cell ss:StyleID=\"{0}\">", c.HeaderStyle.GetStyleId());
                r.AddFormattedLine("<Data ss:Type=\"String\">{0}</Data>", c.HeaderText.XmlEncode());
                r.AppendLine("</Cell>");
            }

            r.AppendLine("</Row>");

            return r.ToString();
        }

        string GenerateDataRows()
        {
            var r = new StringBuilder();

            foreach (var row in DataRows)
            {
                r.AppendLine(@"<Row>");

                for (var i = 0; i < row.Length; i++)
                {
                    var cell = row[i];
                    var column = Columns[i];

                    var cellInfo = cell as Cell;

                    var value = cell?.ToString().OrEmpty();

                    if (column.DataType == "Link")
                    {
                        if (value.IsEmpty())
                        {
                            r.AppendLine("<Cell><Data ss:Type=\"String\"></Data></Cell>");
                        }
                        else
                        {
                            var parts = value.Split(LinkSeperator.ToCharArray().Single());

                            if (parts.Length != 2)
                                throw new Exception("Invalid Link value for ExporttoExcel: " + value);

                            r.AddFormattedLine("<Cell ss:StyleID=\"linkStyle\" ss:HRef=\"{0}\"><Data ss:Type=\"String\">{1}</Data></Cell>",
                                parts[1].XmlEncode(),
                                parts[0].XmlEncode());
                        }
                    }
                    else
                    {
                        if (value.HasValue()) value = value.Remove("\r").XmlEncode().Replace("\n", "&#10;");

                        r.Append("<Cell");

                        var style = cellInfo?.Style ?? column.RowStyle;
                        r.AppendFormat(" ss:StyleID=\"{0}\"", style.GetStyleId());

                        if (column.Formula.HasValue())
                            r.AppendFormat(" ss:Formula=\"{0}\"", column.Formula);

                        r.Append(">");

                        if (value.HasValue())
                            r.AddFormattedLine("<Data ss:Type=\"{0}\">{1}</Data>", column.DataType, value);

                        r.AppendLine("</Cell>");
                    }
                }

                r.AppendLine("</Row>");
            }

            return r.ToString();
        }
    }
}