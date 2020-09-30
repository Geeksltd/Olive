using Olive.Entities;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Csv
{
    /// <summary>
    /// A data-reader style interface for reading Csv files.
    /// </summary>
    public static class CsvReader
    {
        /// <summary>
        /// Reads a CSV blob into a data table. Note use the CastTo() method on the returned DataTable to gain fully-typed objects.
        /// </summary>
        public static async Task<DataTable> ReadAsync(Blob csv, bool isFirstRowHeaders, int minimumFieldCount = 0)
            => Read(await csv.GetContentTextAsync(), isFirstRowHeaders, minimumFieldCount);

        /// <summary>
        /// Reads a CSV file into a data table. Note use the CastTo() method on the returned DataTable to gain fully-typed objects.
        /// </summary>
        public static async Task<DataTable> ReadAsync(FileInfo csv, bool isFirstRowHeaders, int minimumFieldCount = 0)
            => Read(await csv.FullName.AsFile().ReadAllTextAsync(), isFirstRowHeaders, minimumFieldCount);

        /// <summary>
        /// Reads a CSV piece of string into a data table. Note use the CastTo() method on the returned DataTable to gain fully-typed objects.
        /// </summary>
        public static DataTable Read(string csvContent, bool isFirstRowHeaders, int minimumFieldCount = 0)
        {
            var output = new DataTable();

            using (var textReaderContent = new StringReader(csvContent))
            using (var csvResult = new CsvHelper.CsvReader(textReaderContent, System.Globalization.CultureInfo.CurrentCulture))
            {
                csvResult.Read();
                csvResult.ReadHeader();

                var fieldCount = Math.Max(csvResult.Context.HeaderRecord.Length, minimumFieldCount);
                var headers = csvResult.Context.HeaderRecord;

                if (!isFirstRowHeaders)
                    headers = Enumerable.Range(0, fieldCount).Select(i => "Column" + i).ToArray();

                for (var i = 0; i < fieldCount; i++)
                    output.Columns.Add(new DataColumn(headers[i], typeof(string)));

                if (!isFirstRowHeaders)
                {
                    var headerRow = output.NewRow();
                    foreach (DataColumn column in output.Columns)
                        headerRow[column.ColumnName] = csvResult.Context.HeaderRecord[column.Ordinal];
                    output.Rows.Add(headerRow);
                }

                while (csvResult.Read())
                {
                    var row = output.NewRow();
                    foreach (DataColumn column in output.Columns)
                        row[column.ColumnName] = csvResult.GetField(column.DataType, column.Ordinal);
                    output.Rows.Add(row);
                }

                return output;
            }
        }

        /// <summary>
        /// Gets the column names on the specified CSV blob.
        /// </summary>
        public static async Task<string[]> GetColumnsAsync(Blob blob)
            => GetColumns(await blob.GetContentTextAsync());

        /// <summary>
        /// Gets the column names on the specified CSV from a file.
        /// </summary>
        public static async Task<string[]> GetColumnsAsync(FileInfo file)
            => GetColumns(await file.FullName.AsFile().ReadAllTextAsync());

        /// <summary>
        /// Gets the column names on the specified CSV content.
        /// </summary>
        public static string[] GetColumns(string csvContent)
        {
            using (var textReaderContent = new StringReader(csvContent))
            using (var csvResult = new CsvHelper.CsvReader(textReaderContent, System.Globalization.CultureInfo.CurrentCulture))
            {
                csvResult.Read();
                csvResult.ReadHeader();
                return csvResult.Context.HeaderRecord;
            }
        }
    }
}