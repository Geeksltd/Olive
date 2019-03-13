using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Olive.Csv
{
    public static class CsvWriter
    {
        /// <summary>
        /// Saves csv content into a file
        /// </summary>
        /// <param name="this">Csv string</param>
        /// <param name="path">File save path</param>
        public static void Save(this string @this, FileInfo path) => File.WriteAllText(path.FullName, @this);

        /// <summary>
        /// Converts a Dictionary object to CSV string
        /// </summary>
        /// <returns>CSV string content</returns>
        public static string ToCsv(this Dictionary<string, string> @this)
        {
            var csv = new StringBuilder();
            foreach (var pair in @this)
                csv.AppendLine(string.Format("{0},{1}", pair.Key, pair.Value));

            return csv.ToString();
        }

        /// <summary>
        /// Converts an IEnumerable object to CSV string
        /// </summary>
        /// <typeparam name="T">Type of IEnumerable</typeparam> 
        /// <returns>CSV string content</returns>
        public static string ToCsv<T>(this IEnumerable<T> @this)
            where T : class
        {
            var properties = typeof(T).GetProperties();
            var header = properties.Select(p => p.Name.ToCsvValue()).ToString(",");

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine(header);

            foreach (var item in @this)
            {
                var line = properties.Select(p => p.GetValue(item, null).ToCsvValue()).ToString(",");
                csvBuilder.AppendLine(line);
            }

            return csvBuilder.ToString();
        }

        static string ToCsvValue<T>(this T item)
        {
            if (item == null) return "\"\"";

            if (item is string)
                return string.Format("\"{0}\"", item.ToString().Replace("\"", "\"\""));

            if (double.TryParse(item.ToString(), out var dummy))
                return string.Format("{0}", item);

            return string.Format("\"{0}\"", item);
        }

        /// <summary>
        /// Converts a DataTable object to CSV string
        /// </summary>
        /// <returns>CSV string content</returns>
        public static string ToCsv(this DataTable @this)
        {
            var sb = new StringBuilder();
            // headers
            for (var i = 0; i < @this.Columns.Count; i++)
            {
                sb.Append(@this.Columns[i]);
                if (i < @this.Columns.Count - 1) sb.Append(",");
            }

            sb.AppendLine();
            foreach (var dr in @this.GetRows())
            {
                for (var i = 0; i < @this.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                        sb.Append(dr[i].ToString().EscapeCsvValue());

                    if (i < @this.Columns.Count - 1) sb.Append(",");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}