using static Olive.Web.WebRequestLogService;

namespace Olive.Web
{
    public class WebRequestLogMiddleware
    {
        readonly RequestDelegate Next;
        readonly IHttpContextAccessor HttpContextAccessor;

        public WebRequestLogMiddleware(RequestDelegate next, IHttpContextAccessor httpContextAccessor)
        {
            Next = next;
            HttpContextAccessor = httpContextAccessor;
        }

        public async Task Invoke(HttpContext context)
        {
            CurrentRequestLog = WebRequestLogFactory?.Invoke();
            CurrentRequestLog.SessionId = context.Session.Id;

            await Next.Invoke(context);

            await CurrentRequestLog.Perform(async x => await x.Record(HttpContextAccessor.HttpContext));
        }
    }

    public static class WebRequestLogMiddlewareExtension
    {
        public static IApplicationBuilder UseWebRequestLogMiddleware(this IApplicationBuilder builder) =>
            builder.UseMiddleware<WebRequestLogMiddleware>();
    }
}
