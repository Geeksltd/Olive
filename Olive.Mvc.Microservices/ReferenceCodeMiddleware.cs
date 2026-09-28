using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Olive.Mvc.Microservices
{
    /// <summary>
    /// Gives every request a short reference code, which is attached to the context of any log
    /// entry written during that request, and returned to the caller as a response header.
    /// When something fails, the user can be shown the code and support can search the logs for it.
    /// A call from another microservice keeps the code it was sent with, so that the whole user request
    /// is logged under one code across every service it reaches.
    /// </summary>
    public class ReferenceCodeMiddleware
    {
        public const string HeaderName = Log.ReferenceCodeHeader;

        readonly RequestDelegate Next;

        public ReferenceCodeMiddleware(RequestDelegate next) => Next = next;

        public async Task Invoke(HttpContext httpContext)
        {
            // Log owns the shape of a code: work with no HTTP request mints one too, and cannot
            // reference this assembly.
            var code = GetCallerReference(httpContext) ?? Log.NewReferenceCode();

            // Items is what the friendly error page reads to show the code to the user.
            httpContext.Items[Log.ReferenceCodeKey] = code;

            // Setting the header here rather than after Next() so that it survives the exception
            // path (where the response is written by the exception handler) and so that we never
            // attempt to add a header to a response which has already started.
            httpContext.Response.OnStarting(o =>
            {
                var response = ((HttpContext)o).Response;
                if (!response.Headers.ContainsKey(HeaderName))
                    response.Headers[HeaderName] = code;

                return Task.CompletedTask;
            }, httpContext);

            // The scope, not Items, is what the logger reads — the same mechanism that carries a code
            // through a queue handler. Work that is really someone else's opens a nested scope and wins.
            using (Log.UseReference(code))
                await Next(httpContext);
        }

        /// <summary>
        /// The code sent by a calling microservice, when there is one. Only from a caller with a known access
        /// key: anyone can set a header, and a code taken from anyone would let them file their requests
        /// under someone else's code.
        /// </summary>
        static string GetCallerReference(HttpContext httpContext)
        {
            var code = httpContext.Request.Headers[HeaderName].ToString();
            if (!Log.IsReferenceCode(code)) return null;

            var accessKey = httpContext.Request.Headers["Microservice.AccessKey"].ToString();
            return MicroserviceAccessKeyAuthentication.IsKnownClient(accessKey) ? code : null;
        }
    }
}
