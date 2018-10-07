using System;
using System.Threading.Tasks;

namespace Olive
{
    abstract class GetImplementation<TResponse>
    {
        protected ApiClient ApiClient;
        public TResponse Result { get; set; }

        protected ApiFallBackEventPolicy FallBackEventPolicy => ApiClient.FallBackEventPolicy;
        protected AsyncEvent<FallBackEvent> FallBackEvent => ApiClient.FallBack;
        protected TimeSpan? CacheAge => ApiClient.CacheExpiry;

        internal Exception Error;

        protected GetImplementation(ApiClient apiClient)
        {
            ApiClient = apiClient;
        }

        public abstract Task<bool> Attempt(string url);
    }
}