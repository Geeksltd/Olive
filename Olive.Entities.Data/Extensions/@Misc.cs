using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;

namespace Olive.Entities.Data
{
    public static partial class OliveExtensions
    {
        /// <summary>
        /// Returns a MS T-SQL-safe DateTime value for use in queries (i.e. prevents date values earlier than 1/1/1753).
        /// </summary>
        public static DateTime GetSqlSafeValue(this DateTime value) =>
            value < SqlDateTime.MinValue.Value ? SqlDateTime.MinValue.Value : value;

        /// <summary>
        /// Gets a virtual URL to this file. If the file is not in the current website folder it throws an exception.
        /// </summary>
        public static string ToVirtualPath(this FileInfo file)
        {
            if (!file.FullName.StartsWith(AppDomain.CurrentDomain.BaseDirectory, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"The file {file.FullName} is not in the current website folder.");

            var path = "/" + file.FullName.Substring(AppDomain.CurrentDomain.BaseDirectory.Length).TrimStart("\\").TrimStart("/");
            return path.Replace("\\", "/");
        }

        /// <summary>
        /// Returns a DataTable with columns based on the public properties of type T and the rows
        /// populated with the values in those properties for each item in this IEnumerable.
        /// </summary>
        /// <param name="tableName">Optional name for the DataTable (defaults to the plural of the name of type T).</param>
        public static DataTable ToDataTable<T>(this IEnumerable<T> items, string tableName = null)
        {
            var properties = typeof(T).GetProperties();

            var dataTable = new DataTable(tableName.Or(typeof(T).Name.ToPlural()));

            foreach (var property in properties)
                dataTable.Columns.Add(property.Name);

            foreach (T item in items)
            {
                var row = dataTable.NewRow();

                foreach (var property in properties)
                    row[property.Name] = property.GetValue(item);

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        public static int? GetResultsToFetch(this IEnumerable<QueryOption> options) =>
            options.OfType<TakeTopQueryOption>().FirstOrDefault()?.Number;
    }
}
