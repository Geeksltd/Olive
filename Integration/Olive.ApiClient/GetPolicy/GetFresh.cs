using System;
using System.Threading.Tasks;
using static Olive.ApiClient;

namespace Olive
{
    public class GetFresh<T> : IGetImplementation<T>
    {
        readonly FallBackEventPolicy? fallBackEventPolicy;
        readonly AsyncEvent<FallBackEvent> fallBackEvent;

        public GetFresh(FallBackEventPolicy? fallBackEventPolicy, AsyncEvent<FallBackEvent> fallBackEvent)
        {
            this.fallBackEventPolicy = fallBackEventPolicy;
            this.fallBackEvent = fallBackEvent;
        }

        public T Result { get; set; }

        public async Task<bool> Attempt(ApiClient apiClient, string url, TimeSpan? cacheAge, FallBackEventPolicy fallBackEventPolicy) => await ExecuteGet(apiClient, url);

        /// <summary>
        /// Send request to the server and create its cache file
        /// </summary>
        /// <returns>Response</returns>
        async Task<bool> ExecuteGet(ApiClient apiClient, string url)
        {
            try
            {
                var cache = ApiResponseCache<T>.Create(url);

                var requestInfo = new RequestInfo(apiClient) { HttpMethod = "GET" };

                Result = await requestInfo.TrySend<T>();

                if (requestInfo.Error == null)
                {
                    await cache.File.WriteAllTextAsync(requestInfo.ResponseText);

                    return true;
                }
            }
            catch (System.Exception exception)
            {
                Log.For(this).Error(exception);

                if (fallBackEventPolicy.HasValue && fallBackEventPolicy == FallBackEventPolicy.Raise && fallBackEvent != null)
                {
                    await fallBackEvent.Raise(new FallBackEvent
                    {
                        Url = url,
                        FriendlyMessage = $"Failed to get fresh results from {url.AsUri().Host}"
                    });
                }

                return false;
            }

            return false;
        }
    }
}