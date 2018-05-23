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
            if (await ProcessAsWebCommand())
            {
                return;
            }
            else
            {
                await Next.Invoke(context);
            }
        }

        async Task<bool> ProcessAsWebCommand()
        {
            if (TempDatabase.Config?.DatabaseManager == null) return false;

            await TempDatabase.AwaitReadiness();
            var command = Context.Current.Request().Param("Web.Test.Command");
            if (command.IsEmpty()) return false;

            try
            {
                return await WebTestConfig.Run(command);
            }
            catch (Exception ex)
            {
                await Context.Current.Response().EndWith(ex.ToLogString().ToHtmlLines());
                return false;
            }
        }
    }
}
