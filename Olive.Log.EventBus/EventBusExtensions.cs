using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Olive.Logging
{
    public static class EventBusExtensions
    {
        public static ILoggingBuilder AddEventBusLogger(this ILoggingBuilder @this, Action<EventBusLoggerOptions> configure = null)
        {
            @this.Services.AddSingleton<ILoggerProvider, EventBusLoggerProvider>();
            if (configure != null) @this.Services.Configure(configure);
            return @this;
        }
    }
}
