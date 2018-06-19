namespace Olive
{
    public class CachedApiUsageArgs
    {
        public CachedApiUsageArgs() { }

        public CachedApiUsageArgs(string message, string failedUrl, string originalErrorMessage, int cacheAge)
        {
            Message = message;
            FailedUrl = failedUrl;
            OriginalErrorMessage = originalErrorMessage;
            CacheAge = cacheAge;
        }

        public string Message { get; set; }

        public string FailedUrl { get; set; }

        public string OriginalErrorMessage { get; set; }

        public int CacheAge { get; set; }
    }
}