using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Olive.Mvc.Testing
{
    public class WebTestMiddleware
    {
        readonly RequestDelegate Next;

        public WebTestMiddleware(RequestDelegate next)
        {
            Next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await TempDatabase.AwaitReadiness();

            var terminate = false;

            var command = context?.Request?.Param("Web.Test.Command");
            if (command.HasValue())
            {
                try
                {
                    terminate = await WebTestConfig.Run(command);
                }
                catch (Exception ex)
                {
                    await context.Response.EndWith(ex.ToLogString().ToHtmlLines());
                    return;
                }
            }

            if (!terminate) await Next.Invoke(context);
        }
    }
}
