using System;

namespace Olive
{
    public partial class ApiClient
    {
        [Obsolete("I Think this event is deprecated")]
        public static readonly AsyncEvent<ApiResponseCache> UsingCacheInsteadOfFresh = new AsyncEvent<ApiResponseCache>();

        public static readonly AsyncEvent<FallBackEvent> FallBackEvent = new AsyncEvent<FallBackEvent>();

        public string Url { get; private set; }
        public ApiClient(string apiUrl) => Url = apiUrl;

        [Obsolete("This property is deprecated, please use FallBackEventPolicy.")]
        public OnApiCallError ErrorAction { get; private set; } = OnApiCallError.Throw;

        public FallBackEventPolicy FallBackEventPolicy { get; private set; }

        [Obsolete("This method is deprecated, please use OnFallBack()")]
        public ApiClient OnError(OnApiCallError onError)
        {
            ErrorAction = onError;

            return this;
        }

        public ApiClient OnFallBack(FallBackEventPolicy fallBackEventPolicy)
        {
            FallBackEventPolicy = fallBackEventPolicy;
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