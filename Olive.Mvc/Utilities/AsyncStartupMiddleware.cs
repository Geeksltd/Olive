using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

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
                await Context.OnStartedUp();
            }

            await Next.Invoke(context);
        }
    }
}
