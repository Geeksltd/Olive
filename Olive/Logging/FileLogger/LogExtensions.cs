using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Olive
{
    public static class LogExtensions
    {
        public static ILoggingBuilder AddFile(this ILoggingBuilder @this, string filePrefix = "log-")
        {
            @this.AddFile(options => options.FilePrefix = filePrefix);
            return @this;
        }

        public static ILoggingBuilder AddFile(this ILoggingBuilder @this, Action<FileLoggerOptions> configure = null)
        {
            @this.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();

            if (configure != null) @this.Services.Configure(configure);

            return @this;
        }
    }
}
