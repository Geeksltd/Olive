using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public class DevCommandMiddleware
    {
        readonly IServiceProvider ServiceProvider;
        readonly RequestDelegate Next;
        ITempDatabase TempDatabase;

        public DevCommandMiddleware(IServiceProvider serviceProvider,
            ITempDatabase tempDatabase, RequestDelegate next)
        {
            ServiceProvider = serviceProvider;
            TempDatabase = tempDatabase;
            Next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (TempDatabase != null)
                await TempDatabase.AwaitReadiness();

            var path = context.Request.Path.Value.TrimStart("/");
            if (!path.StartsWith("cmd/")) await Next.Invoke(context);
            else
            {
                var command = path.TrimStart("cmd/");
                if (await InvokeCommand(command)) return;
                else
                {
                    var urlReferrer = context.Request.Headers["Referer"].ToString();
                    if (urlReferrer.IsEmpty()) context.Response.EndWith("Commands ran successfully.");
                    else context.Response.Redirect(urlReferrer);
                }
            }
        }

        async Task<bool> InvokeCommand(string command)
        {
            if (command.IsEmpty())
            {
                await Context.Current.Response()
                    .EndWith(
                    "Available commands:\n\n" +
                    AvailableCommands().Select(x => "/cmd/" + x.Name).ToLinesString());
                return true;
            }

            var commands = AvailableCommands().Where(x => x.Name == command);

            if (commands.None())
                await Context.Current.Response().EndWith("Command not registered: " + command);

            return await commands.Select(x => x.Run()).AwaitAll().ToArray().Any();
        }

        IEnumerable<IDevCommand> AvailableCommands()
        {
            return ServiceProvider
                 .GetServices<IDevCommand>()
                 .Where(x => x.IsEnabled());
        }
    }
}