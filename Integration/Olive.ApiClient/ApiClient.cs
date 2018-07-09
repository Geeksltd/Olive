namespace Olive
{
    public partial class ApiClient
    {
        /// <summary>
        /// Raised when a less desirable cache policy option is used. For example if for FreshOrCacheOrFail, there is no fresh data available, but data from cache is successfully returned, this event will be fired.
        /// </summary>
        public static readonly AsyncEvent<FallBackEvent> FallBack = new AsyncEvent<FallBackEvent>();

        public string Url { get; private set; }
        public ApiClient(string apiUrl) => Url = apiUrl;

        public ApiFallBackEventPolicy FallBackEventPolicy { get; private set; } = ApiFallBackEventPolicy.Raise;

        public ApiClient OnFallBack(ApiFallBackEventPolicy fallBackEventPolicy)
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