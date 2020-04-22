using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace @Olive.PassiveBackgroundTasks
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
            try
            {
                Log.For(this).Info("Running background tasks ...");
                await BackgroundProcessManager.Run();
                Log.For(this).Info("Finished running background tasks ...");
            }
            catch (Exception ex)
            {
                Log.For(this).Error(ex, "Failed to run the background tasks because : " + ex.ToFullMessage());
            }
        }
    }
}
