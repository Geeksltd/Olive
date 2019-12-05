using Hangfire;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.Hangfire.Serverless
{
    class DistributedBackgroundTasksMiddleware
    {
        private readonly RequestDelegate Next;

        public DistributedBackgroundTasksMiddleware(RequestDelegate _next)
        {
            Next = _next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var cancellationResource = new CancellationTokenSource();
            var token = cancellationResource.Token;

            using (var server = new BackgroundJobServer())
            {
                await Task.Delay(TimeSpan.FromSeconds(5), token); // Optional delay to catch some more background jobs
                server.SendStop();
                await server.WaitForShutdownAsync(token);
            }
        }
    }
}
