using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Olive.Mvc
{
    class LogUnhandledExceptionsMiddleware : BaseMiddleware
    {
        public LogUnhandledExceptionsMiddleware(RequestDelegate next) : base(next)
        {
        }

        public override async Task Invoke(HttpContext context)
        {
            try
            {
                await Next.Invoke(context);
            }
            catch (Exception e)
            {
                if (context.Response.StatusCode >= 500)
                {
                    var message = "[Unhandled exception] " + context.Request.ToRawUrl();
                    Log.For<LogUnhandledExceptionsMiddleware>().Error(e, message);
                }
                throw;
            }
        }
    }

    public static class HandleExceptionsMiddlewareExtensions
    {
        [Obsolete("ASP.NET already logs every unhandled exception once: the exception handler or developer page " +
            "when it catches it, and the server when it escapes. This middleware can only log it again.")]
        public static IApplicationBuilder UseLogUnhandledExceptionsMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<LogUnhandledExceptionsMiddleware>();
            return app;
        }
    }
}