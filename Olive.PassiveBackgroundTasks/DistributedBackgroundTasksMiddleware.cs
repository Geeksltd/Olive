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

        public Task Invoke(HttpContext httpContext) => BackgroundProcessManager.Run();
    }
}
