using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Http;
using Olive.Entities.Data;
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
            Console.WriteLine("Initializing DistributedBackgroundTasksMiddleware");
            Next = _next;
            Console.WriteLine("Initialized DistributedBackgroundTasksMiddleware");
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var start = LocalTime.Now;
            Console.WriteLine("Invoking DistributedBackgroundTasksMiddleware at " + start);

            JobStorage.Current = new SqlServerStorage(Context.Current.GetService<IConnectionStringProvider>().GetConnectionString());

            var cancellationResource = new CancellationTokenSource();
            var token = cancellationResource.Token;

            using (var server = new BackgroundJobServer())
            {
                await Task.Delay(TimeSpan.FromSeconds(Config.Get<int>("Automated.Tasks:Action.Delay", 5)), token); // Optional delay to catch some more background jobs
                server.SendStop();
                await server.WaitForShutdownAsync(token);
            }

            httpContext.Response.ContentType = "text/plain";
            httpContext.Response.Clear();
            httpContext.Response.Write("Done");
            Console.WriteLine("Invoked DistributedBackgroundTasksMiddleware after " + LocalTime.Now.Subtract(start).ToNaturalTime());
        }
    }
}
