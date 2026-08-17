using System;

namespace Olive.Entities.Data
{
    /// <summary>
    /// An optional capability for an ICacheProvider to also cache the results of filtered
    /// queries (GetList()/Count() with criteria), keyed by a query signature.
    /// Providers that don't implement this (e.g. RedisCacheProvider) simply don't get this feature.
    /// </summary>
    public interface IQueryCacheProvider : ICacheProvider
    {
        object GetQueryResult(Type type, string key);
        void SetQueryResult(Type type, string key, object result);
        void RemoveQueryResults(Type type);
    }
}
