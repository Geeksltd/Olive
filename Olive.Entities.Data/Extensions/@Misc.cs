using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
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
