using System;

namespace Olive
{
    public partial class ApiClient
    {
        [Obsolete("I Think this event is deprecated")]
        public static readonly AsyncEvent<ApiResponseCache> UsingCacheInsteadOfFresh = new AsyncEvent<ApiResponseCache>(); //TODO: Should be deleted

        public static readonly AsyncEvent<ApiClientEvent> ApiClientEvent = new AsyncEvent<ApiClientEvent>();
        public static readonly AsyncEvent<UsingCacheInsteadOfFreshEvent> UsingCacheInsteadOfFreshEvent = new AsyncEvent<UsingCacheInsteadOfFreshEvent>();

        public string Url { get; private set; }
        public ApiClient(string apiUrl) => Url = apiUrl;

        [Obsolete("This property is deprecated, please use FallBack.")]
        public OnApiCallError ErrorAction { get; private set; } = OnApiCallError.Throw;

        public FallBack FallBack { get; private set; }

        [Obsolete("This method is deprecated, please use OnFallBack()")]
        public ApiClient OnError(OnApiCallError onError)
        {
            ErrorAction = onError;

            return this;
        }

        public ApiClient OnFallBack(FallBack fallBack)
        {
            FallBack = fallBack;
            return this;
        }
    }

    internal class ServerError
    {
        public string Message { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionType { get; set; }
        public string StackTrace { get; set; }
    }
}