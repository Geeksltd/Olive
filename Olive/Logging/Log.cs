﻿using System;
using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Olive
{
    public static class LogExtensions
    {
        public static ILoggingBuilder AddFile(this ILoggingBuilder @this, Action<FileLoggerOptions> configure = null)
        {
            @this.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
            if (configure != null) @this.Services.Configure(configure);
            return @this;
        }

        public static void Error(this ILogger @this, string message)
            => @this.LogError(message);

        public static void Error(this ILogger @this, Exception ex, string message = null)
            => @this.LogError(ex, message.Or(ex.ToLogString()));

        public static void Warning(this ILogger @this, string message, object relatedObject = null, string userId = null, string userIp = null)
        {
            @this.LogWarning(ToYaml(message, relatedObject, userId, userIp));
        }

        public static void Debug(this ILogger @this, string message, object relatedObject = null, string userId = null, string userIp = null)
        {
            @this.LogDebug(ToYaml(message, relatedObject, userId, userIp));
        }

        public static void Info(this ILogger @this, string message, object relatedObject = null, string userId = null, string userIp = null)
        {
            @this.LogInformation(ToYaml(message, relatedObject, userId, userIp));
        }

        public static void Audit(this ILogger @this, string message, object relatedObject = null, string userId = null, string userIp = null)
        {
            @this.LogTrace(ToYaml(message, relatedObject, userId, userIp));
        }

        static string ToYaml(string description, object relatedObject, string userId, string userIp)
        {
            var r = new StringBuilder();
            if (userId.HasValue()) r.AppendLine($"  UserId: {userId}");
            if (userIp.HasValue()) r.AppendLine($"  UserIP: {userIp}");
            if (relatedObject != null) r.AppendLine($"  Object: {relatedObject.ToStringOrEmpty()}");

            if (description.HasValue())
            {
                r.Append("  Description: ");
                var firstLine = true;

                foreach (var line in description.ToLines().Trim())
                {
                    if (!firstLine) r.Append("  Description: ".Length);
                    r.AppendLine(line);
                    firstLine = false;
                }
            }

            return r.ToString();
        }
    }
    
    public static class Log
    {
        public static ILoggerFactory Factory { get; private set; }
       
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Init(Action<ILoggingBuilder> configure)
        {
            if(Factory != null)return;
            Factory = Microsoft.Extensions.Logging.LoggerFactory.Create(configure);
        }
       
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Init(ILoggerFactory factory)
        {
            if (Factory != null)return;
            Factory = factory;
        }

        /// <summary>
        /// A shortcut to Context.Current.GetService«ILogger»().
        /// </summary>
        public static ILogger For(Type type) => (Factory ?? (Factory = Context.Current.GetService<ILoggerFactory>())).CreateLogger(type);

        public static ILogger For<TType>() => For(typeof(TType));

        /// <summary>
        /// A shortcut to Context.Current.GetService«ILogger»().
        /// </summary>
        public static ILogger For(object useThis) => For(useThis?.GetType() ?? typeof(Log));
    }
}