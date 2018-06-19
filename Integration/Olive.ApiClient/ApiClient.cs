namespace Olive
{
    public partial class ApiClient
    {
        public static readonly AsyncEvent<CachedApiResponse> UsingCacheInsteadOfFresh = new AsyncEvent<CachedApiResponse>();
        public string Url { get; private set; }
        public ApiClient(string apiUrl) => Url = apiUrl;

        public OnApiCallError ErrorAction { get; private set; } = OnApiCallError.Throw;

        public ApiClient OnError(OnApiCallError onError)
        {
            ErrorAction = onError;
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