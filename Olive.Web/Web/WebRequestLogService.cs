namespace Olive.Web
{
    /// <summary>
    /// Provides services to web request log objects.
    /// </summary>
    public static class WebRequestLogService
    {
        static bool IsEnabled = Config.Get<bool>("Enable.Request.Logging", defaultValue: false);

        #region Factory

        /// <summary>
        /// Specifies a factory to instantiate WebRequestLog objects.
        /// </summary>
        public static Func<IWebRequestLog> WebRequestLogFactory = CreateWebRequestLog;

        static Type concreteWebRequestLogType;
        static IWebRequestLog CreateWebRequestLog()
        {
            if (concreteWebRequestLogType != null)
            {
                var result = Activator.CreateInstance(concreteWebRequestLogType) as IWebRequestLog;
                result.Start = LocalTime.Now;
                return result;
            }

            var possible = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.References(typeof(IWebRequestLog).Assembly))
                .SelectMany(a => a.GetExportedTypes().Where(t => t.Implements<IWebRequestLog>())).ToList();

            if (possible.Count == 0)
                throw new Exception("No type in the currently loaded assemblies implements IWebRequestLog.");

            if (possible.Count > 1)
                throw new Exception("More than one type in the currently loaded assemblies implement IWebRequestLog:" + possible.Select(x => x.FullName).ToString(" and "));

            concreteWebRequestLogType = possible.Single();
            return CreateWebRequestLog();
        }

        #endregion

        /// <summary>
        /// Records this web request log in the provided http context.
        /// </summary>
        public static async Task Record(this IWebRequestLog log, HttpContext context)
        {
            try
            {
                await DoRecord(log, context);
            }
            catch (Exception ex)
            {
                Log.Error("Could not record Web Request Log", ex);
            }
        }

        static async Task DoRecord(IWebRequestLog log, HttpContext context)
        {
            log.ProcessingTime = LocalTime.Now.Subtract(log.Start).TotalMilliseconds;

            log.Url = context.Request.ToRawUrl();
            log.UserAgent = context.Request.Headers["User-Agent"].ToString();
            log.IP = context.Connection.RemoteIpAddress.ToString();
            log.RequestType = context.Request.Method;
            log.UrlReferer = context.Request.Headers["Referer"].ToString();
            log.SessionId = context.Session?.Id;

            if (context.Response.ContentLength.HasValue)
                log.ResponseLength = (int)(context.Response.ContentLength.Value / 1024);

            await Entity.Database.Save(log);
        }

        public static string FindSearchKeywords(this IWebRequestLog log)
        {
            if (log.UrlReferer.IsEmpty()) return null;

            // var url = UrlReferer.ToLower();

            if (!log.UrlReferer.ToLower().Contains(".google.co"))
                return null;

            foreach (var possibleQuerystringKey in new[] { "q", "as_q" })
            {
                var query = new Uri(log.UrlReferer).Query.TrimStart("?").OrEmpty().Split('&').Trim().FirstOrDefault(p => p.StartsWith(possibleQuerystringKey + "="));

                if (query.HasValue())
                {
                    return HttpUtility.UrlDecode(query.Substring(1 + possibleQuerystringKey.Length));
                }
            }

            return log.UrlReferer;
        }

        /// <summary>
        /// Gets the number of requests made in the same session.
        /// </summary>
        public static async Task<int> CountRequestsInSession(this IWebRequestLog log) =>
            await Entity.Database.Count<IWebRequestLog>(r => r.SessionId == log.SessionId);

        /// <summary>
        /// Gest the last url visited in this session.
        /// </summary>
        public static async Task<string> GetLastVisitedUrl(this IWebRequestLog log)
        {
            return (await Entity.Database.GetList<IWebRequestLog>(r => r.SessionId == log.SessionId)).WithMax(r => r.Start).Url;
        }

        /// <summary>
        /// Gets the value of a query string key.
        /// </summary>
        public static string GetData(this IWebRequestLog log, string key)
        {
            var query = new Uri(log.Url).Query?.TrimStart("?").Split('&').FirstOrDefault(p => p.StartsWith(key + "="));

            if (query.IsEmpty()) return null;
            else
                return HttpUtility.UrlDecode(query.Substring(1 + key.Length));
        }

        /// <summary>
        /// Gets the first request of every session that has had an activity during the last 10 minutes.
        /// </summary>
        public static async Task<IEnumerable<IWebRequestLog>> FindRecentSessions(TimeSpan since)
        {
            var startDate = LocalTime.Now.Subtract(since);

            var sessions = (await Entity.Database.GetList<IWebRequestLog>(r => r.Start > startDate)).GroupBy(r => r.SessionId);

            return sessions.Select(session => session.WithMin(r => r.Start)).ToArray();
        }

        public static IWebRequestLog CurrentRequestLog
        {
            get { return Context.HttpContextAccessor.HttpContext.Items["Current.Request.Log"] as IWebRequestLog; }
            set { Context.HttpContextAccessor.HttpContext.Items.Add("Current.Request.Log", value); }
        }
    }
}