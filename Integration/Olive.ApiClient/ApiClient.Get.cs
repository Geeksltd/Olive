using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Olive
{
    partial class ApiClient
    {
        static ConcurrentDictionary<string, AsyncLock> GetLocks =
            new ConcurrentDictionary<string, AsyncLock>();

        public string StaleDataWarning = "The latest data cannot be received from the server right now.";


        CachePolicy CachePolicy = CachePolicy.FreshOrCacheOrFail;
        TimeSpan? CacheExpiry;

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
                queryString = queryParams.GetType().GetPropertiesAndFields(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name + "=" + p.GetValue(queryParams).ToStringOrEmpty().UrlEncode())
                     .Trim().ToString("&");

            if (queryString.LacksAll()) return Url;

            if (Url.Contains("?")) return (Url + "&" + queryString).KeepReplacing("&&", "&");
            return Url + "?" + queryString;
        }



        public async Task<TResponse> Get<TResponse>(object queryParams = null)
        {
            Url = GetFullUrl(queryParams);
            Log.For(this).Debug("Get: Url = " + Url);

            var urlLock = GetLocks.GetOrAdd(Url, x => new AsyncLock());

            var cache = CachedApiResponse<TResponse>.Create(Url);

            using (await urlLock.Lock())
            {
                if (CachePolicy == CachePolicy.CacheOrFreshOrFail && await cache.HasValidValue(CacheExpiry))
                {
                    Log.For(this).Debug("Get: Returning from Cache: " + cache.File.Name);
                    return cache.Data;
                }

                // Not already cached:
                return await ExecuteGet<TResponse>();
            }
        }

        async Task<TResponse> ExecuteGet<TResponse>()
        {
            var cache = CachedApiResponse<TResponse>.Create(Url);

            var request = new RequestInfo(this) { HttpMethod = "GET" };

            var result = await request.TrySend<TResponse>();
            if (request.Error == null)
            {
                await cache.File.WriteAllTextAsync(request.ResponseText);
                return result;
            }
            else
            {
                if (CachePolicy == CachePolicy.FreshOrCacheOrFail && await cache.HasValidValue(/*Ignore expiry*/))
                {
                    if (ErrorAction == OnApiCallError.IgnoreAndNotify)
                        await UsingCacheInsteadOfFresh.Raise(cache);

                    return cache.Data;
                }
                else throw request.Error;
            }
        }

        /// <summary>
        /// Deletes all cached Get API results.
        /// </summary>
        public static Task DisposeCache() => CachedApiResponse.DisposeAll();

        /// <summary>
        /// Deletes the cached Get API result for the specified API url.
        /// </summary>
        public Task DisposeCache<TResponse>(string getApiUrl)
            => CachedApiResponse<TResponse>.Create(getApiUrl).File.DeleteAsync(harshly: true);
    }
}