using System;
using System.Threading.Tasks;

namespace Olive
{
    public class GetNull<T> : IGetImplementation<T>
    {
        readonly FallBackEventPolicy? fallBackEventPolicy;
        readonly AsyncEvent<FallBackEvent> fallBackEvent;

        public GetNull(FallBackEventPolicy? fallBackEventPolicy, AsyncEvent<FallBackEvent> fallBackEvent)
        {
            this.fallBackEventPolicy = fallBackEventPolicy;
            this.fallBackEvent = fallBackEvent;
        }

        public T Result { get; set; }

        public async Task<bool> Attempt(ApiClient apiClient, string url, TimeSpan? cacheAge, FallBackEventPolicy fallBackEventPolicy)
        {
            Result = default(T);

            if (this.fallBackEventPolicy.HasValue && this.fallBackEventPolicy == FallBackEventPolicy.Raise && fallBackEvent != null)
            {
                await fallBackEvent.Raise(new FallBackEvent
                {
                    Url = url,
                    CacheAge = cacheAge,
                    FriendlyMessage = $"Failed to get fresh or cache results from {url.AsUri().Host}."
                });
            }

            return true;
        }
    }
}