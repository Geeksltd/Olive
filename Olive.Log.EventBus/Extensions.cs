using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Log.EventBus
{
    public static class Extensions
    {
        public static ILoggingBuilder AddEventBusLogger(this ILoggingBuilder @this, Action<EventBusLoggerOptions> configure = null)
        {
            @this.Services.AddSingleton<ILoggerProvider, EventBusLoggerProvider>();
            if (configure != null) @this.Services.Configure(configure);
            return @this;
        }
    }
}
