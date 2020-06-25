using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Olive
{
    public static partial class OliveEntitiesExtensions
    {
        /// <summary>
        /// Gets the root entity type of this type.
        /// If this type inherits directly from Entity&lt;T&gt; then it will be returned, otherwise its parent...
        /// </summary>
        public static Type GetRootEntityType(this Type objectType)
        {
            var baseType = objectType.BaseType;
            if (baseType == null)
                throw new NotSupportedException(objectType.FullName + " not recognised. It must be a subclass of Olive.Entities.Entity.");

            if (baseType.Name == "GuidEntity") return objectType;
            if (baseType == typeof(Entity<int>)) return objectType;
            if (baseType == typeof(Entity<long>)) return objectType;
            if (baseType == typeof(Entity<string>)) return objectType;

            return GetRootEntityType(baseType);
        }

        /// <summary>
        /// Downloads the data in this URL.
        /// </summary>
        public static async Task<Blob> DownloadBlob(this Uri url, string cookieValue = null, int timeOutSeconds = 60)
        {
            var fileName = "File.Unknown";

            if (url.IsFile)
                fileName = url.ToString().Split('/').Last();

            return new Blob(await url.DownloadData(cookieValue, timeOutSeconds), fileName);
        }

        public static IDatabase Database(this Context @this) => @this.GetService<IDatabase>();

        public static int? GetResultsToFetch(this IEnumerable<QueryOption> options) =>
          options.OfType<TakeTopQueryOption>().FirstOrDefault()?.Number;

        /// <summary>
        /// Returns a MS T-SQL-safe DateTime value for use in queries (i.e. prevents date values earlier than 1/1/1753).
        /// </summary>
        public static DateTime GetSqlSafeValue(this DateTime value) =>
            value < SqlDateTime.MinValue.Value ? SqlDateTime.MinValue.Value : value;

        /// <summary>
        /// Executes the specified command text against the database connection of the context and builds an IDataReader.
        /// The command type will be `CommandType.Text`.
        /// Make sure you close the data reader after finishing the work.
        /// </summary>
        public static Task<IDataReader> ExecuteReader(this IDataAccess @this, string command, params object[] parameters)
        {
            var dataParams = CreateParameters(@this, command, parameters);
            return @this.ExecuteReader(command, CommandType.Text, dataParams);
        }

        /// <summary>
        /// Executes the specified command text against the database connection of the context and returns the single value.
        /// The command type will be `CommandType.Text`.
        /// </summary>
        public static Task<object> ExecuteScalar(this IDataAccess @this, string command, params object[] parameters)
        {
            var dataParams = CreateParameters(@this, command, parameters);
            return @this.ExecuteScalar(command, CommandType.Text, dataParams);
        }

        /// <summary>
        /// Executes the specified command text as nonquery.
        /// The command type will be `CommandType.Text`.
        /// </summary>
        public static Task<int> ExecuteNonQuery(this IDataAccess @this, string command, params object[] parameters)
        {
            var dataParams = CreateParameters(@this, command, parameters);
            return @this.ExecuteNonQuery(command, CommandType.Text, dataParams);
        }

        static IDataParameter[] CreateParameters(IDataAccess @this, string command, object[] parameters)
        {
            var expectedParams = Regex.Matches(command, "\\@([a-z|A-Z|\\d|_]+)");

            if (expectedParams.Count != parameters.Length)
                throw new InvalidOperationException("An incorrect number of parameters passed.");

            var dataParams = new IDataParameter[parameters.Length];

            for (var index = 0; index < parameters.Length; index++)
                dataParams[index] = @this.CreateParameter(expectedParams[index].Value, parameters[index]);
            return dataParams;
        }

        /// <summary>
        /// Maps each record in the reader to an object using the provided mapper fuction.
        /// </summary>
        /// <typeparam name="T">Result elemets type</typeparam>
        /// <param name="mapper">Mapper fuction to create each item. Is should not call the `reader.read()`</param>
        public static Task<IEnumerable<T>> Select<T>(this Task<IDataReader> @this, Func<IDataReader, T> mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            return Select(@this, mapper, null);
        }

        /// <summary>
        /// Maps each record in the reader to an object using the provided mapper fuction.
        /// </summary>
        /// <typeparam name="T">Result elemets type</typeparam>
        /// <param name="mapper">Mapper fuction to create each item. Is should not call the `reader.read()`</param>
        public static Task<IEnumerable<T>> SelectAsync<T>(this Task<IDataReader> @this, Func<IDataReader, Task<T>> mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            return Select(@this, null, mapper);
        }

        static async Task<IEnumerable<T>> Select<T>(this Task<IDataReader> @this, Func<IDataReader, T> mapper, Func<IDataReader, Task<T>> asyncMapper)
        {
            var reader = await @this;

            var result = new List<T>();

            if (asyncMapper == null)
                while (reader.Read())
                    result.Add(mapper(reader));
            else
                while (reader.Read())
                    result.Add(await asyncMapper(reader));

            if (!reader.IsClosed) reader.Close();

            return result;
        }
    }
}