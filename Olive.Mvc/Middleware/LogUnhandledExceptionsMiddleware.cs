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
                    Log.For<LogUnhandledExceptionsMiddleware>().Error(e);
                }
                throw;
            }
        }
    }

    public static class HandleExceptionsMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogUnhandledExceptionsMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<LogUnhandledExceptionsMiddleware>();
            return app;
        }
    }
}