using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Olive.Mvc
{
    class AsyncStartupMiddleware
    {
        static bool IsStarted;
        readonly RequestDelegate Next;
        Func<Task> Startup;

        public AsyncStartupMiddleware(RequestDelegate next, Func<Task> startup)
        {
            Next = next;
            Startup = startup;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!IsStarted)
            {
                IsStarted = true;
                await Startup();
                await Context.StartedUp.Raise();
            }

            await Next.Invoke(context);
        }
    }
}
