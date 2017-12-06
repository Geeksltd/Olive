namespace Olive.Web
{
    [LogEvents(false), CacheObjects(false)]
    public interface IWebRequestLog : IEntity
    {
        string IP { get; set; }
        double ProcessingTime { get; set; }
        string RequestType { get; set; }
        int ResponseLength { get; set; }
        string SearchKeywords { get; }
        string SessionId { get; set; }
        DateTime Start { get; set; }
        string Url { get; set; }
        string UrlReferer { get; set; }
        string UserAgent { get; set; }
    }

    public static class WebRequestLogExtensions
    {
        public static async Task<int> CountSessionRequests(this IWebRequestLog request)
        {
            if (request.SessionId.IsEmpty()) return 1;
            return await Entity.Database.Count<IWebRequestLog>(x => x.SessionId == request.SessionId);
        }

        public static async Task<string> GetLastVisitedUrl(this IWebRequestLog request)
        {
            if (request.SessionId.IsEmpty()) return request.Url;

            return (await Entity.Database.GetList<IWebRequestLog>(x => x.SessionId == request.SessionId)).WithMax(x => x.Start)?.Url;
        }

        public static TimeSpan GetDuration(this IWebRequestLog request) => TimeSpan.FromMilliseconds(request.ProcessingTime);

        public static async Task<bool> IsBouncedBack(this IWebRequestLog request) => await request.CountSessionRequests() == 1;
    }
}
