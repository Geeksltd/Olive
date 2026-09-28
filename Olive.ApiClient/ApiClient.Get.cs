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

            Log.For(this).Debug("Get: Url = " + Url.Split('?')[0]);

            var urlLock = GetLocks.GetOrAdd(Url, x => new AsyncLock());

            using (await urlLock.Lock())
            {
                Exception firstError = null;
                var freshFailed = false;

                // get result according to the cache policy
                foreach (var implementor in CachePolicy.GetImplementors<TResponse>(this))
                {
                    if (await implementor.Attempt(Url))
                    {
                        if (freshFailed) LogFallBack(implementor, firstError);
                        return implementor.Result;
                    }

                    freshFailed |= implementor is GetFresh<TResponse>;
                    firstError = firstError ?? implementor.Error;
                }

                if (firstError != null) throw firstError;
                else return default(TResponse);
            }
        }

        /// <summary>
        /// Says that a result is not fresh because getting one failed. The failure itself is logged as a
        /// warning, as it may yet be recovered from; here it was not, and the caller cannot tell, so this
        /// is the error.
        /// </summary>
        void LogFallBack<TResponse>(GetImplementation<TResponse> implementor, Exception error)
        {
            var given = implementor is GetCache<TResponse> ? "a cached result" : "no result";
            Log.For(this).Error(error, $"Returned {given} for GET {Url.Split('?')[0]}, as getting a fresh one failed.");
        }

        /// <summary>
        /// Deletes all cached Get API results.
        /// </summary>
        public static Task DisposeCache() => ApiResponseCache.DisposeAll();

        /// <summary>
        /// Deletes the cached Get API result for the specified API url.
        /// </summary>
        public Task DisposeCache<TResponse>(string getApiUrl)
            => ApiResponseCache<TResponse>.Create(getApiUrl).Delete();
    }
}