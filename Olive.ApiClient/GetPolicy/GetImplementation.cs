using System;
using System.Threading.Tasks;

namespace Olive
{
    abstract class GetImplementation<TResponse>
    {
        protected ApiClient ApiClient;
        public TResponse Result { get; set; }

        protected ApiFallBackEventPolicy FallBackEventPolicy => ApiClient.FallBackEventPolicy;
        protected event AwaitableEventHandler<FallBackEvent> FallBackEvent;
        protected TimeSpan? CacheAge => ApiClient.CacheExpiry;

        internal Exception Error;

        protected GetImplementation(ApiClient apiClient)
        {
            ApiClient = apiClient;
        }

        internal Task OnFallBackEvent(FallBackEvent args) => FallBackEvent.Raise(args);

        public abstract Task<bool> Attempt(string url);
    }
}