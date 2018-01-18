using System;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Entities
{
    public static partial class OliveExtensions
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
    }
}