using System;
using System.Collections.Concurrent;

namespace Olive.Entities
{
    /// <summary>
    /// Marks a small reference data type whose records are all loaded together, in a single query, the first time
    /// any of them is requested by its ID and is not already in the database cache. Later lookups by ID, and
    /// Include() of associations to this type, are then served from the cache while the loaded list remains in it.
    /// <para/>
    /// The loaded list is kept for as long as the database cache keeps it: one request in the multi-server cache mode,
    /// or until a record of this type is saved or deleted in the single-server mode. It is not used inside a
    /// transaction, within a DatabaseContext, when the type is not cacheable, or when soft delete is bypassed. Nor outside
    /// a request in the multi-server cache mode, where each call gets a new database cache that would not keep the list.
    /// <para/>
    /// A lookup by an ID that is not in the loaded list (e.g. a soft deleted record, or one added since) still loads
    /// that record by its ID. Queries with criteria, Count() and Any() are not affected.
    /// This attribute is not inherited: each type that needs this behaviour must be marked.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CacheAllRecordsAttribute : Attribute
    {
        static readonly ConcurrentDictionary<Type, bool> Cache = new ConcurrentDictionary<Type, bool>();

        /// <summary>
        /// Determines whether a specified type is marked with CacheAllRecords.
        /// </summary>
        public static bool IsEnabled(Type type) =>
            Cache.GetOrAdd(type, t => t.IsDefined(typeof(CacheAllRecordsAttribute), inherit: false));
    }
}
