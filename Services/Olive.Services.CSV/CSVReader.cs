using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Olive.Entities;

namespace Olive.Services.CSV
{
    /// <summary>
    /// A data-reader style interface for reading Csv files.
    /// </summary>
    public static class CsvReader
    {
        /// <summary>
        /// Reads a CSV blob into a data table. Note use the CastTo() method on the returned DataTable to gain fully-typed objects.
        /// </summary>
        public static async Task<DataTable> Read(Blob csv, bool isFirstRowHeaders, int minimumFieldCount = 0) =>
            Read(await csv.GetContentTextAsync(), isFirstRowHeaders, minimumFieldCount);

        /// <summary>
        /// Reads a CSV file into a data table. Note use the CastTo() method on the returned DataTable to gain fully-typed objects.
        /// </summary>
        public static async Task<DataTable> Read(FileInfo csv, bool isFirstRowHeaders, int minimumFieldCount = 0) =>
            Read(await File.ReadAllTextAsync(csv.FullName), isFirstRowHeaders, minimumFieldCount);

        /// <summary>
        /// Reads a CSV piece of string into a data table. Note use the CastTo() method on the returned DataTable to gain fully-typed objects.
        /// </summary>
        public static DataTable Read(string csvContent, bool isFirstRowHeaders, int minimumFieldCount = 0)
        {
            var output = new DataTable();

            using (var csv = new LumenWorks.Framework.IO.Csv.CsvDataReader(new StringReader(csvContent), isFirstRowHeaders))
            {
                csv.MissingFieldAction = LumenWorks.Framework.IO.Csv.MissingFieldAction.ReplaceByNull;
                var fieldCount = Math.Max(csv.FieldCount, minimumFieldCount);
                var headers = csv.GetFieldHeaders();

                if (!isFirstRowHeaders)
                    headers = Enumerable.Range(0, fieldCount).Select(i => "Column" + i).ToArray();

                for (int i = 0; i < fieldCount; i++)
                    output.Columns.Add(new DataColumn(headers[i], typeof(string)));

                while (csv.ReadNextRecord())
                {
                    var row = output.NewRow();

                    for (int i = 0; i < fieldCount; i++) row[i] = csv[i];

                    output.Rows.Add(row);
                }
            }

            return output;
        }

        /// <summary>
        /// Gets the column names on the specified CSV blob.
        /// </summary>
        public static async Task<string[]> GetColumns(Blob blob) => GetColumns(await blob.GetContentTextAsync());

        /// <summary>
        /// Gets the column names on the specified CSV content.
        /// </summary>
        public static string[] GetColumns(string csvContent)
        {
            using (var csv = new LumenWorks.Framework.IO.Csv.CsvDataReader(new StringReader(csvContent), true))
                return csv.GetFieldHeaders();
        }
    }
}