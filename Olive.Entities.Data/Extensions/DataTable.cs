﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Olive.Entities.Data
{
    public static partial class DataExtensions
    {
        /// <summary>
        /// Casts this data table's records into a list of typed objects.        
        /// </summary>
        public static IEnumerable<T> CastTo<T>(this DataTable dataTable) where T : new() =>
            CastTo<T>(dataTable, null);

        /// <summary>
        /// Casts this data table's records into a list of typed objects.
        /// <param name="propertyMappings">An anonymouse object containing property mapping information.
        /// e.g.: new {Property1 = "Property name in CSV", Property2 = "...", set_Property1 = new Func&lt;string, object&gt;(text => Client.Parse(value)) }</param>
        /// </summary>
        public static IEnumerable<T> CastTo<T>(this DataTable dataTable, object propertyMappings) where T : new() =>
            CastAsDictionary<T>(dataTable, propertyMappings).Select(i => i.Key).ToList();

        /// <summary>
        /// Casts this data table's records into a list of typed objects.
        /// <param name="propertyMappings">An anonymouse object containing property mapping information.
        /// e.g.: new {Property1 = "Property name in CSV", Property2 = "...", set_Property1 = new Func&lt;string, object&gt;(text => Client.Parse(value)) }</param>
        /// </summary>
        public static Dictionary<T, DataRow> CastAsDictionary<T>(this DataTable data, object propertyMappings) where T : new()
        {
            if (propertyMappings != null)
                foreach (var p in propertyMappings.GetType().GetProperties())
                {
                    if (p.PropertyType == typeof(string)) continue;

                    if (p.PropertyType == typeof(Func<string, object>))
                    {
                        if (!p.Name.StartsWith("set_"))
                            throw new ArgumentException("Property convertors must start with 'set_{property name}'");

                        continue;
                    }

                    throw new ArgumentException($"Unrecognized value for the property {p.PropertyType} of the specified propertyMappings");
                }

            var mappings = FindPropertyMappings(typeof(T), data.Columns, propertyMappings);

            var convertors = new Dictionary<string, Func<string, object>>();
            if (propertyMappings != null)
                convertors = propertyMappings.GetType().GetProperties().Where(p => p.PropertyType == typeof(Func<string, object>))
                .ToDictionary(p => p.Name.Substring(4), p => (Func<string, object>)p.GetValue(propertyMappings));

            var result = new Dictionary<T, DataRow>();

            foreach (DataRow record in data.Rows)
            {
                var item = ParseObject<T>(record, mappings, convertors);
                result.Add(item, record);
            }

            return result;
        }

        /// <summary>
        /// Finds the property mappings for the specified target type, CSV column names and user declared mappings.
        /// </summary>
        static Dictionary<string, string> FindPropertyMappings(Type targetType, DataColumnCollection columns, object declaredMappings)
        {
            var result = new Dictionary<string, string>();

            if (declaredMappings != null)
            {
                foreach (var property in declaredMappings.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (property.Name.StartsWith("set_"))
                    {
                        if (!result.ContainsKey(property.Name.TrimStart("set_")))
                            result.Add(property.Name.TrimStart("set_"), null);
                        continue;
                    }

                    // Validate property name:
                    var propertyInTarget = targetType.GetProperty(property.Name);
                    if (propertyInTarget == null)
                        throw new Exception(targetType.FullName + " does not have a property named " + property.Name);

                    if (!propertyInTarget.CanWrite)
                        throw new Exception("{0}.{1} property is read-only.".FormatWith(targetType.FullName, property.Name));

                    var mappedName = (string)property.GetValue(declaredMappings);
                    result[property.Name] = mappedName;
                }
            }

            var columnNames = columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();

            foreach (var property in targetType.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                if (!property.CanWrite) continue;

                if (result.ContainsKey(property.Name) && result[property.Name] != null)
                    continue; // Already added in explicit mappings.

                // Otherwise, if a column with that name is available, then that's it:
                var potential = columnNames.Where(c => c.Replace(" ", "").ToLower() == property.Name.ToLower());
                if (potential.IsSingle())
                {
                    result[property.Name] = potential.Single();
                }
                else if (potential.Any())
                {
                    throw new Exception("The specified data contains multiple potential matches for the property '{0}'. The potentially matched columns found: {1}. You must use explicit mappings in this case."
                        .FormatWith(property.Name, potential.Select(c => $"'{c}'").ToString(", ")));
                }
            }

            return result;
        }

        /// <summary>
        /// Creates an object of the specified type with the specified data and property mappings.
        /// </summary>
        static T ParseObject<T>(DataRow dataContainer, Dictionary<string, string> propertyMappings, Dictionary<string, Func<string, object>> convertors)
        {
            var result = Activator.CreateInstance<T>();

            foreach (var mapping in propertyMappings)
            {
                var property = result.GetType().GetProperty(mapping.Key);

                string data;

                if (mapping.Value == null)
                    // The setter for this property is identified, while no property mapping is specified.
                    data = null;
                else
                    data = dataContainer[mapping.Value]?.ToString().TrimOrNull();

                try
                {
                    object dataToSet;

                    if (convertors.ContainsKey(mapping.Key))
                        dataToSet = convertors[mapping.Key](data);
                    else
                        dataToSet = data.To(property.PropertyType);

                    property.SetValue(result, dataToSet);
                }
                catch (Exception ex)
                {
                    throw new Exception("Could not set the value of the property '{0}' from the value of '{1}'.".FormatWith(mapping.Key, data), ex);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the CSV data equivalent to this data table.
        /// </summary>
        public static string ToCSV(this DataTable table)
        {
            var result = new StringBuilder();
            for (var i = 0; i < table.Columns.Count; i++)
            {
                result.Append(table.Columns[i].ColumnName);
                result.Append(i == table.Columns.Count - 1 ? "\n" : ",");
            }

            foreach (DataRow row in table.Rows)
            {
                for (var i = 0; i < table.Columns.Count; i++)
                {
                    result.Append(row[i].ToString());
                    result.Append(i == table.Columns.Count - 1 ? "\n" : ",");
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Gets the rows of this data table in a LINQ-able format..
        /// </summary>
        public static IEnumerable<DataRow> GetRows(this DataTable dataTable) => dataTable.Rows.Cast<DataRow>();

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

            foreach (var item in items)
            {
                var row = dataTable.NewRow();

                foreach (var property in properties)
                    row[property.Name] = property.GetValue(item);

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }
}