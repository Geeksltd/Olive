using Olive;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Olive.Export
{
    public partial class Exporter<T>
    {
        const int LINK_SEPRATOR_CHAR_CODE = 166, MAX_LENGTH_FOR_SUMMARIZE = 31;
        public List<Column<T>> Columns = new List<Column<T>>();
        public List<object[]> DataRows = new List<object[]>();

        public Exporter(string documentName)
        {
            DocumentName = documentName;
            HeaderGroupBackgroundColor = HeaderBackGroundColor = "#CCCCCC";
            HeaderFontName = "Arial";
        }

        /// <summary>
        /// Creates a new Exporter instance for a data table.
        /// It automatically configures the exporter for all columns and rows of the data table.
        /// </summary>
        public Exporter(System.Data.DataTable dataTable)
        {
            if (dataTable == null)
                throw new ArgumentNullException(nameof(dataTable));

            DocumentName = dataTable.TableName;
            HeaderBackGroundColor = "#CCCCCC";

            foreach (System.Data.DataColumn column in dataTable.Columns)
                AddColumn(column.ColumnName);// TODO: Add data type when necessary

            foreach (System.Data.DataRow row in dataTable.Rows)
                AddRow(row.ItemArray);
        }

        public static string LinkSeperator => Convert.ToChar(LINK_SEPRATOR_CHAR_CODE).ToString();

        public string DocumentName { get; set; }

        public string HeaderBackGroundColor { get; set; }

        public string HeaderFontName { get; set; }

        public string HeaderGroupBackgroundColor { get; set; }

        public bool FreezeHeader { get; set; }

        public bool FreezeFirstColumn { get; set; }

        public double DefaultColumnWidth { get; set; }

        public bool ExcludeHeader { get; set; }

        public Column<T> GetColumn(string headerText)
            => Columns.FirstOrDefault(x => x.HeaderText == headerText);

        public Column<T> AddColumn(string headerText) => AddColumn(headerText, "String");

        public Column<T> AddColumn(string headerText, string type)
            => AddColumn(headerText, type, default(Func<T, object>));

        public Column<T> AddColumn(string headerText, string type, Func<T, object> data)
        {
            if (headerText.IsEmpty())
                throw new ArgumentNullException(nameof(headerText));

            if (type.IsEmpty())
                throw new ArgumentNullException(nameof(type));

            var result = new Column<T>(headerText, type) { Data = data };
            Columns.Add(result);
            return result;
        }

        public void RemoveColumn(string headerText)
        {
            var columns = Columns.Where(c => c.HeaderText == headerText);
            if (columns.HasMany())
                throw new ArgumentException($"There are {columns.Count()} columns with header text of '{headerText}'. Please use RemoveColumn(index) instead.");

            if (columns.None())
                throw new ArgumentException($"There is no column with header text of '{headerText}'.");

            RemoveColumn(Columns.IndexOf(columns.Single()));
        }

        public void RemoveColumn(Column<T> column) => RemoveColumn(Columns.IndexOf(column));

        public void RemoveColumn(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex > Columns.Count - 1)
                throw new ArgumentException("columnIndex should be between 0 and " + (Columns.Count - 1));

            Columns.RemoveAt(columnIndex);

            for (var i = 0; i < DataRows.Count; i++)
                DataRows[i] = DataRows[i].Where((r, ind) => ind != columnIndex).ToArray();
        }

        /// <summary>
        /// Adds a data row to the excel output.
        /// <param name="dataCells">Either Cell instances or value objects.</param>
        /// </summary>
        public void AddRow(params object[] dataCells)
        {
            if (dataCells == null)
                throw new ArgumentNullException(nameof(dataCells));

            if (Columns.All(x => x.GroupName.IsEmpty()))
            {
                if (dataCells.Length != Columns.Count())
                    throw new ArgumentException($"The number of row cell values does not match the number of columns ({dataCells.Length} <> {Columns.Count()})");
            }
            else
            {
                // Do we need validation for grouping mode?
            }

            DataRows.Add(dataCells);
        }

        public void AddRows(IEnumerable<T> dataItems)
        {
            if (dataItems == null)
                throw new ArgumentNullException(nameof(dataItems));

            foreach (var column in Columns.Where(c => c.Data == null))
                throw new Exception($"Column.Data should be specified for Exporter.AddRows() method to work. For '{column.HeaderText}' it is null.");

            foreach (var item in dataItems)
            {
                var dataCells = new List<object>();

                foreach (var column in Columns)
                {
                    try
                    {
                        dataCells.Add(column.Data(item));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Invoking the Data evaluator for excel column '{column.HeaderText}' failed on {item.GetType().Name} instance: '{item}'", ex);
                    }
                }

                AddRow(dataCells.ToArray());
            }
        }

        public Column<T> AddDropDownColumn(string headerText, string type, string enumerationName, IEnumerable<object> possibleValues)
        {
            if (headerText.IsEmpty())
                throw new ArgumentNullException(nameof(headerText));

            if (type.IsEmpty())
                throw new ArgumentNullException(nameof(type));

            if (possibleValues == null)
                throw new ArgumentNullException(nameof(possibleValues));

            var result = new DropDownColumn<T>(headerText, type, enumerationName, possibleValues.ToArray());
            Columns.Add(result);

            return result;
        }

        public string Generate(Format format)
        {
            switch (format)
            {
                case Format.Csv: return GenerateCsv();
                case Format.ExcelXml: return GenerateExcelXml(this);
                default: throw new NotSupportedException();
            }
        }

        string GenerateCsv()
        {
            var r = new StringBuilder();

            // Header row:
            if (!ExcludeHeader)
                r.AppendLine(Columns.Select(c => c.HeaderText.EscapeCsvValue()).ToString(","));

            // Data rows:
            foreach (var row in DataRows)
            {
                var fields = new List<string>();
                for (var i = 0; i < row.Length; i++)
                {
                    var cell = row[i];
                    var column = Columns[i];

                    var value = cell?.ToString().OrEmpty();

                    if (column.DataType == "Link")
                    {
                        if (value.IsEmpty())
                            fields.Add(value);
                        else
                        {
                            var parts = value.Split(LinkSeperator.ToCharArray().Single());

                            if (parts.Length != 2)
                                throw new Exception("Invalid Link value for ExporttoExcel: " + value);

                            fields.Add(parts[0] + ": " + parts[1]);
                        }
                    }
                    else
                    {
                        fields.Add(value);
                    }
                }

                r.AppendLine(fields.Select(f => f.EscapeCsvValue()).ToString(","));
            }

            return r.ToString();
        }

        /// <summary>
        /// Gets the file extension for a specified output format.
        /// </summary>
        public string GetFileExtension(Format output)
        {
            switch (output)
            {
                case Format.ExcelXml: return ".xls";
                case Format.Csv: return ".csv";
                default:
                    throw new NotSupportedException();
            }
        }

        public Blob ToDocument(Format type)
            => new Blob(Generate(type).GetUtf8WithSignatureBytes(), DocumentName + GetFileExtension(type));
    }
}