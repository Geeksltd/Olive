using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Olive.Logging
{
    public static class EventBusExtensions
    {
        public static ILoggingBuilder AddEventBus(this ILoggingBuilder @this, Action<EventBusLoggerOptions> configure = null)
        {
            Console.WriteLine("Registering Olive.Log.EventBus");
            @this.Services.AddSingleton<ILoggerProvider, EventBusLoggerProvider>();
            if (configure != null) @this.Services.Configure(configure);
            Console.WriteLine("Registered Olive.Log.EventBus");
            return @this;
        }
    }
}
