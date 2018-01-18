using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Olive.Web;

namespace Olive.Services.Testing
{
    public class WebTestMiddleware
    {
        readonly RequestDelegate Next;

        public WebTestMiddleware(RequestDelegate next) => Next = next;

        public async Task Invoke(HttpContext context)
        {
            TempDatabase.AwaitReadiness();

            var terminate = false;

            var command = context?.Request?.Param("Web.Test.Command");
            if (command.HasValue())
                terminate = await WebTestConfig.Run(command);

            if (!terminate) await Next.Invoke(context);
        }
    }
}
