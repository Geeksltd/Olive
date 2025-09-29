namespace Olive
{
    using Microsoft.AspNetCore.Builder;
    using Newtonsoft.Json;

    public static class EventBusMvcExtensions
    {
        public static IApplicationBuilder RegisterCommandConsumerProcessUrl(this IApplicationBuilder app)
        {
            app.Map("/olive/process-command", builder =>
            {
                builder.Run(async x =>
                {
                    var commandTypeFullName = x.Request.Path.Value?.TrimStart('/');

                    if (commandTypeFullName.IsEmpty())
                        throw new ArgumentException("CommandName is required in the URL.");

                    var type = AppDomain.CurrentDomain
                        .GetAssemblies()
                        .Select(a => a.GetType(commandTypeFullName, throwOnError: false))
                        .FirstOrDefault(t => t != null);

                    if (type is null)
                        throw new ArgumentException($"Command with type '{commandTypeFullName}' not found.");

                    await Olive.EventBus.Queue(type).PullAll(x =>
                    {
                        var message = JsonConvert.DeserializeObject(x, type) as EventBusCommandMessage;
                        return message?.Process() ?? Task.CompletedTask;
                    });

                    x.Response.Write("Done");
                });
            });

            return app;
        }

    }
}
