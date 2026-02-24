using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Security.Claims;
using System.Text;

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

        /// <summary>
        /// When set, provides contextual information (e.g. UserId, RequestUrl, UserIP) to append to log entries.
        /// </summary>
        public static Func<string> ContextProvider { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Init(Action<ILoggingBuilder> configure = null)
        {
            if (Factory != null) return;

            var configuration = Config.Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                if (configure != null)
                    configure(builder);
            });

            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            Log.Init(loggerFactory);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Init(ILoggerFactory factory)
        {
            if (Factory != null) return;
            Factory = factory;
            InitDefaultContextProvider();
        }

        static void InitDefaultContextProvider()
        {
            var httpContextAccessorType = Type.GetType(
                "Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.Abstractions");

            if (httpContextAccessorType == null) return;

            var httpContextProp = httpContextAccessorType.GetProperty("HttpContext");

            ContextProvider = () =>
            {
                try
                {
                    var accessor = Context.Current.GetOptionalService(httpContextAccessorType);
                    if (accessor == null) return null;

                    var httpContext = httpContextProp?.GetValue(accessor);
                    if (httpContext == null) return null;

                    var contextType = httpContext.GetType();

                    // User info
                    var user = contextType.GetProperty("User")?.GetValue(httpContext) as ClaimsPrincipal;
                    var userId = user?.GetId();
                    var userEmail = user?.GetEmail();

                    // Request info
                    var request = contextType.GetProperty("Request")?.GetValue(httpContext);
                    string requestUrl = null, httpMethod = null, userAgent = null, traceId = null;
                    if (request != null)
                    {
                        var reqType = request.GetType();
                        var pathBase = reqType.GetProperty("PathBase")?.GetValue(request)?.ToString();
                        var path = reqType.GetProperty("Path")?.GetValue(request)?.ToString();
                        var queryString = reqType.GetProperty("QueryString")?.GetValue(request)?.ToString();
                        requestUrl = $"{pathBase}{path}{queryString}";
                        httpMethod = reqType.GetProperty("Method")?.GetValue(request)?.ToString();

                        var headers = reqType.GetProperty("Headers")?.GetValue(request);
                        if (headers != null)
                        {
                            var indexer = headers.GetType().GetProperty("Item", new[] { typeof(string) });
                            userAgent = indexer?.GetValue(headers, new object[] { "User-Agent" })?.ToString();
                        }
                    }

                    // Connection info
                    var connection = contextType.GetProperty("Connection")?.GetValue(httpContext);
                    var userIp = connection?.GetType().GetProperty("RemoteIpAddress")?.GetValue(connection)?.ToString();

                    // Trace identifier
                    traceId = contextType.GetProperty("TraceIdentifier")?.GetValue(httpContext)?.ToString();

                    if (userId.IsEmpty() && requestUrl.IsEmpty() && userIp.IsEmpty()) return null;

                    var r = new StringBuilder();
                    if (userId.HasValue()) r.AppendLine($"  UserId: {userId}");
                    if (userEmail.HasValue()) r.AppendLine($"  UserEmail: {userEmail}");
                    if (httpMethod.HasValue()) r.AppendLine($"  HttpMethod: {httpMethod}");
                    if (requestUrl.HasValue()) r.AppendLine($"  RequestUrl: {requestUrl}");
                    if (userIp.HasValue()) r.AppendLine($"  UserIP: {userIp}");
                    if (userAgent.HasValue()) r.AppendLine($"  UserAgent: {userAgent}");
                    if (traceId.HasValue()) r.AppendLine($"  TraceId: {traceId}");
                    return r.ToString().TrimEnd();
                }
                catch { return null; }
            };
        }
        public static bool AddProvider<TProvider>() where TProvider : ILoggerProvider
        {
            if (Factory == null)
                throw new InvalidOperationException("LoggerFactory is not initialized. Call Log.Init() first.");
            var registeredProvider = Context.Current.GetOptionalService<TProvider>();
            if (registeredProvider == null) return false;
            Factory.AddProvider(registeredProvider);
            return true;
        }

        /// <summary>
        /// A shortcut to Context.Current.GetService«ILogger»().
        /// </summary>
        public static ILogger For(Type type)
        {
            if (Factory == null)
                throw new InvalidOperationException("LoggerFactory is not initialized. Call Log.Init() first.");
            return Factory.CreateLogger(type);
        }

        public static ILogger For<TType>() => For(typeof(TType));

        /// <summary>
        /// A shortcut to Context.Current.GetService«ILogger»().
        /// </summary>
        public static ILogger For(object useThis) => For(useThis?.GetType() ?? typeof(Log));
    }
}