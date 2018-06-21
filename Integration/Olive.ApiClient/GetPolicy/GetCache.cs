using System;
using System.Threading.Tasks;

namespace Olive
{
    public class GetCache<T> : IGetImplementation<T>
    {
        readonly FallBackEventPolicy? fallBackEventPolicy;
        readonly AsyncEvent<FallBackEvent> fallBackEvent;

        public GetCache(FallBackEventPolicy? fallBackEventPolicy, AsyncEvent<FallBackEvent> fallBackEvent)
        {
            this.fallBackEventPolicy = fallBackEventPolicy;
            this.fallBackEvent = fallBackEvent;
        }

        public T Result { get; set; }

        public async Task<bool> Attempt(ApiClient apiClient, string url, TimeSpan? cacheAge, FallBackEventPolicy fallBackEventPolicy)
        {
            var cache = ApiResponseCache<T>.Create(url);

            if (await cache.HasValidValue(cacheAge))
            {
                Result = cache.Data;

                if (this.fallBackEventPolicy.HasValue && this.fallBackEventPolicy == FallBackEventPolicy.Raise && fallBackEvent != null)
                {
                    var age = cacheAge.HasValue ? LocalTime.UtcNow.Subtract(cacheAge.Value).DaysInMonth() : 0;

                    await fallBackEvent.Raise(new FallBackEvent
                    {
                        Url = url,
                        CacheAge = cacheAge,
                        FriendlyMessage = $"Failed to get fresh results from {url.AsUri().Host} .using the latest cache from {age} days ago."
                    });
                }

                return true;
            }

            return false;
        }
    }
}