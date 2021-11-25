using System;

namespace Olive
{
    public enum CachePolicy
    {
        /// <summary>
        /// Choose this if you definitely need up-to-date, but can live with more potential for crashing, resource inefficient and reduced speed.
        /// A fresh request will be sent always. If it fails, an error will be thrown. Any cache will be ignored.
        /// </summary>
        FreshOrFail,
        /// <summary>
        /// Choose this if you definitely need up-to-date, but can live with more potential for crashing, resource inefficient and reduced speed.
        /// A fresh request will be sent always. If it fails, null will be returned.
        /// </summary>
        FreshOrNull,
        /// <summary>
        /// Choose this if you prefer Up-to-date, Minimum crashing, but not fast or resource efficient.
        /// It makes a fresh HTTP request and it waits for the response. If the result was successful, the cache will be updated. If it fails, if there is a valid cache version, that would be returned and if there is no valid cache, null will be returned. This option is safe, is more likely to give you up to date result, but it's not the fastest option as it always waits for a remote call before returning.
        /// </summary>
        FreshOrCacheOrNull,
        /// <summary>
        /// Choose this if you prefer Up-to-date, Minimum crashing, but not fast or resource efficient.
        /// It makes a fresh HTTP request and it waits for the response. If result was successful, cache will be updated. If it fails, but we already have a cached result from before, that will be returned instead of showing an error. This option is safe, is more likely to give you up to date result, but it's not the fastest option as it always waits for a remote call before returning.
        /// </summary>
        FreshOrCacheOrFail,
        /// <summary>
        /// Choose this if you prefer fast, minimum crashing, resource efficient, but not up-to-date.
        /// If a cache is available from before, it will be returned and no fresh request will be sent  and if there is no valid cache it will make a HTTP request. This is the fastest option, but the result can be old.
        /// If it fails, an error will be thrown. 
        /// </summary>
        CacheOrFreshOrFail,
        /// <summary>
        /// Choose this if you prefer fast, minimum crashing, resource efficient, but not up-to-date.
        /// If a cache is available from before, it will be returned and no fresh request will be sent and if there is no valid cache it will make a HTTP request. This is the fastest option, but the result can be old.
        /// If it fails, Null will be returned. 
        /// </summary>
        CacheOrFreshOrNull,
        /// <summary>
        /// Choose this if you prefer fast, minimum crashing, resource efficient, but not up-to-date.
        /// If there is no valid cache, Null will be returned.
        /// </summary>
        CacheOrNull,
        /// <summary>
        /// Choose this if you prefer fast, minimum crashing, resource efficient, but not up-to-date.
        /// If it fails, an error will be thrown. 
        /// </summary>
        CacheOrFail
    }

    public static class CachePolicyExtention
    {
        internal static GetImplementation<T>[] GetImplementors<T>(this CachePolicy policy, ApiClient client)
        {
            GetImplementation<T> fresh() => new GetFresh<T>(client);
            GetImplementation<T> cache() => new GetCache<T>(client, silent: true);
            GetImplementation<T> fail() => new GetFail<T>();
            GetImplementation<T> @null() => new GetNull<T>(client);

            GetImplementation<T> cacheRaise() => new GetCache<T>(client, silent: false);

            return policy switch
            {
                CachePolicy.FreshOrNull => new[] { fresh(), @null() },
                CachePolicy.FreshOrFail => new[] { fresh(), fail() },
                CachePolicy.FreshOrCacheOrNull => new[] { fresh(), cacheRaise(), @null() },
                CachePolicy.FreshOrCacheOrFail => new[] { fresh(), cacheRaise(), fail() },
                CachePolicy.CacheOrNull => new[] { cache(), @null() },
                CachePolicy.CacheOrFail => new[] { cache(), fail() },
                CachePolicy.CacheOrFreshOrNull => new[] { cache(), fresh(), @null() },
                CachePolicy.CacheOrFreshOrFail => new[] { cache(), fresh(), fail() },
                _ => throw new Exception("Cache policy has not implemented yet!"),
            };
        }
    }
}