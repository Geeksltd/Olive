using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        public DevCommandMiddleware(IServiceProvider serviceProvider, RequestDelegate next)
        {
            ServiceProvider = serviceProvider;
            Next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await (ServiceProvider.GetService<ITempDatabase>()?.AwaitReadiness() ?? Task.CompletedTask);

            var path = context.Request.Path.Value.TrimStart("/");
            if (!path.StartsWith("cmd/"))
            {
                await Next.Invoke(context);
                return;
            }

            var command = path.TrimStart("cmd/");
            var response = await InvokeCommand(command);

            if (response.HasValue())
            {
                await context.Response.EndWith(response);
                return;
            }

            if (context.Response.HasStarted) return;

            var urlReferrer = context.Request.Headers["Referer"].ToString();
            if (urlReferrer.IsEmpty()) context.Response.Redirect("/");
            else context.Response.Redirect(urlReferrer);
        }

        ILogger Logger => Log.For<DevCommandMiddleware>();

        async Task<string> InvokeCommand(string command)
        {
            if (command.IsEmpty())
            {
                return "<html><body><h3>Available commands:</h3><ul>" +
                       AvailableCommands()
                       .Select(x => $"<li>{x.Title}: <a href='/cmd/{x.Name}'>/cmd/{x.Name}</a></li>")
                       .ToLinesString() + "</ul></body></html>";
            }

            var commands = AvailableCommands().Where(x => x.Name == command);
            if (commands.None()) return "Dev command not registered: " + command;

            if (command.HasMany())
                Logger.Warning("Multiple dev command implementations found for: " + command);
            else
                Logger.Info("Running Dev Command: " + command);

            return await commands.Last().Run();
        }

        IEnumerable<IDevCommand> AvailableCommands()
        {
            return ServiceProvider
                 .GetServices<IDevCommand>()
                 .Where(x => x.IsEnabled());
        }
    }
}