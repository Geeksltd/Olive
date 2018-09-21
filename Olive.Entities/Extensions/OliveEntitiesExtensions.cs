using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
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
    }
}