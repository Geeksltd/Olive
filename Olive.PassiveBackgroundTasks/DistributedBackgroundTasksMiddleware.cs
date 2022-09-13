using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace @Olive.PassiveBackgroundTasks
{
    class DistributedBackgroundTasksMiddleware
    {
        readonly RequestDelegate _next;

        public DistributedBackgroundTasksMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                var force = httpContext.Request.Query.ContainsKey("force");
                var taskName = httpContext.Request.Query.ContainsKey("key")
                    ? httpContext.Request.Query["key"].ToString()
                    : null;

                var report = await Engine.Run(force, taskName).ConfigureAwait(false);

                httpContext.Response.ContentType = "text/html; charset=utf-8";
                await httpContext.Response.WriteAsync(string.Join("<br/>", report));
            }
            catch (Exception ex)
            {
                var message = "Failed to run the background tasks because : " + ex.ToFullMessage();
                Log.For(this).Error(ex, message);
                await httpContext.Response.WriteAsync(message);
            }
        }
    }
}