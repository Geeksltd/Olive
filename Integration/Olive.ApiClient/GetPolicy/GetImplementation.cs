using System;
using System.Threading.Tasks;

namespace Olive
{
    abstract class GetImplementation<TResponse>
    {
        protected ApiClient ApiClient;
        public TResponse Result { get; set; }

        protected FallBackEventPolicy FallBackEventPolicy => ApiClient.FallBackEventPolicy;
        protected AsyncEvent<FallBackEvent> FallBackEvent => ApiClient.FallBackEvent;
        protected TimeSpan? CacheAge => ApiClient.CacheExpiry;

        protected GetImplementation(ApiClient apiClient)
        {
            ApiClient = apiClient;
        }

        public abstract Task<bool> Attempt(string url);
    }
}