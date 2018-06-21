using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive
{
    partial class ApiClient
    {
        static readonly ConcurrentDictionary<string, AsyncLock> GetLocks = new ConcurrentDictionary<string, AsyncLock>();

        CachePolicy CachePolicy = CachePolicy.FreshOrCacheOrFail;
        internal TimeSpan? CacheExpiry;

        public ApiClient Cache(CachePolicy policy, TimeSpan? cacheExpiry = null)
        {
            CachePolicy = policy;
            CacheExpiry = cacheExpiry;
            return this;
        }

        string GetFullUrl(object queryParams = null)
        {
            if (queryParams == null) return Url;

            var queryString = queryParams as string;

            if (queryString is null)
            {
                queryString = queryParams.GetType().GetPropertiesAndFields(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name + "=" + p.GetValue(queryParams).ToStringOrEmpty().UrlEncode())
                    .Trim().ToString("&");
            }

            if (queryString.LacksAll()) return Url;

            if (Url.Contains("?")) return (Url + "&" + queryString).KeepReplacing("&&", "&");
            return Url + "?" + queryString;
        }

        public async Task<TResponse> Get<TResponse>(object queryParams = null)
        {
            Url = GetFullUrl(queryParams);

            Log.For(this).Debug("Get: Url = " + Url);

            var urlLock = GetLocks.GetOrAdd(Url, x => new AsyncLock());

            using (await urlLock.Lock())
            {
                // get result according to the cache policy
                foreach (var implementor in CachePolicy.GetImplementors<TResponse>(this))
                {
                    if (await implementor.Attempt(Url))
                    {
                        return implementor.Result;
                    }
                }

                return default(TResponse);
            }
        }

        /// <summary>
        /// Deletes all cached Get API results.
        /// </summary>
        public static Task DisposeCache() => ApiResponseCache.DisposeAll();

        /// <summary>
        /// Deletes the cached Get API result for the specified API url.
        /// </summary>
        public Task DisposeCache<TResponse>(string getApiUrl)
            => ApiResponseCache<TResponse>.Create(getApiUrl).File.DeleteAsync(harshly: true);
    }
}