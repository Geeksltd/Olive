using System.Linq;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    /// <summary>
    /// Provides extension methods for finding records by their ID, and loading all records of a [CacheAllRecords] type.
    /// </summary>
    public static class DatabaseFindExtensions
    {
        /// <summary>
        /// Gets the record with the specified ID, or null if there is no such record.
        /// It is served from the cache when possible, and for a [CacheAllRecords] type all its records are loaded
        /// together on the first cache miss.
        /// <para/>
        /// Unlike GetOrDefault(), database errors are not swallowed. This relies on the data provider reporting a
        /// missing record with a DataException (as the ADO.NET providers do) or with null. For an interface type,
        /// or when this IDatabase is not Olive's Database, it behaves like GetOrDefault().
        /// </summary>
        public static async Task<T> FindById<T>(this IDatabase @this, object id) where T : IEntity
        {
            if (@this is Database database) return (T)await database.FindById(id, typeof(T));

            return (T)await @this.GetOrDefault(id, typeof(T));
        }

        /// <summary>
        /// Returns all records of a [CacheAllRecords] type, loading them together into the database cache if not
        /// already loaded. Returns null when they cannot be kept in the cache now, e.g. inside a transaction, when
        /// soft delete is bypassed or the type is not cacheable, in which case the caller should query the database
        /// for what it needs, rather than load all records every time.
        /// </summary>
        public static async Task<T[]> TryLoadAllRecords<T>(this IDatabase @this) where T : IEntity
        {
            if (!(@this is Database database) || !database.CanLoadAllRecords(typeof(T))) return null;

            return (await database.LoadAllRecords(typeof(T))).Cast<T>().ToArray();
        }
    }
}
