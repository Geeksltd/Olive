namespace Olive
{
    using Microsoft.AspNetCore.Builder;

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

                    // Through the static, not a hand-rolled deserialize-and-invoke: that is the one place
                    // that opens the reference scope and reports a failure inside it. Dispatching here
                    // instead, the command would log under the code of the request draining the queue.
                    await Olive.EventBus.Queue(type).PullAll(body => EventBusCommandMessage.Process(body, type));

                    x.Response.Write("Done");
                });
            });

            return app;
        }

    }
}
