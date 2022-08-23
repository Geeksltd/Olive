using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Olive.Mvc
{
    class HandleExceptionsMiddleware : BaseMiddleware
    {
        public HandleExceptionsMiddleware(RequestDelegate next) : base(next)
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
                    Log.For<HandleExceptionsMiddleware>().Error(e);
                }
                throw;
            }
        }
    }

    public static class HandleExceptionsMiddlewareExtensions
    {
        public static IApplicationBuilder UseHandleExceptions(this IApplicationBuilder app)
        {
            app.UseMiddleware<HandleExceptionsMiddleware>();
            return app;
        }
    }
}