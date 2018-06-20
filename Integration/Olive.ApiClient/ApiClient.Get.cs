using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive
{
    partial class ApiClient
    {
        private static readonly ConcurrentDictionary<string, AsyncLock> GetLocks = new ConcurrentDictionary<string, AsyncLock>();

        CachePolicy _cachePolicy = CachePolicy.FreshOrCacheOrFail;
        TimeSpan? _cacheExpiry;

        public ApiClient Cache(CachePolicy policy, TimeSpan? cacheExpiry = null)
        {
            _cachePolicy = policy;
            _cacheExpiry = cacheExpiry;
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
                //all available cache policy state
                GetResponse<TResponse> getResponse = new FreshOrFail<TResponse>(this, Url, _cachePolicy);
                GetResponse<TResponse> freshOrNull = new FreshOrNull<TResponse>(this, Url, _cachePolicy);
                GetResponse<TResponse> freshOrCacheOrNull = new FreshOrCacheOrNull<TResponse>(this, Url, _cachePolicy, _cacheExpiry, FallBack, UsingCacheInsteadOfFreshEvent);
                GetResponse<TResponse> freshOrCacheOrFail = new FreshOrCacheOrFail<TResponse>(this, Url, _cachePolicy, _cacheExpiry, FallBack, UsingCacheInsteadOfFreshEvent);
                GetResponse<TResponse> cacheOrFreshOrFail = new CacheOrFreshOrFail<TResponse>(this, Url, _cachePolicy, _cacheExpiry);
                GetResponse<TResponse> cacheOrFreshOrNull = new CacheOrFreshOrNull<TResponse>(this, Url, _cachePolicy, _cacheExpiry);
                GetResponse<TResponse> cacheOrNull = new CacheOrNull<TResponse>(this, Url, _cachePolicy, _cacheExpiry);

                getResponse.SetSuccessor(freshOrNull);
                freshOrNull.SetSuccessor(freshOrCacheOrNull);
                freshOrCacheOrNull.SetSuccessor(freshOrCacheOrFail);
                freshOrCacheOrFail.SetSuccessor(cacheOrFreshOrFail);
                cacheOrFreshOrFail.SetSuccessor(cacheOrFreshOrNull);
                cacheOrFreshOrNull.SetSuccessor(cacheOrNull);

                try
                {
                    var result = await getResponse.Execute();

                    if (FallBack == Olive.FallBack.Warn)
                    {
                        await ApiClientEvent.Raise(new ApiClientEvent { Url = Url, CacheAge = _cacheExpiry, FriendlyMessage = "Content loaded successfully" });
                        //await UsingCacheInsteadOfFresh.Raise(new ApiResponseCache<TResponse>() { Message = "Sample ok result" });
                    }

                    return result;
                }
                catch (Exception exception)
                {

                    if (FallBack == Olive.FallBack.Warn)
                    {
                        await ApiClientEvent.Raise(new ApiClientEvent
                        {
                            Url = Url,
                            CacheAge = _cacheExpiry,
                            FriendlyMessage = "Error happend, please try again" + exception.Message,
                            ExceptionMessage = exception.Message
                        });
                    }
                    else
                    {
                        Log.For(this).Error(exception);
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