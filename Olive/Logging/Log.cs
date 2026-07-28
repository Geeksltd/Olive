using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Olive
{
    public static class LogExtensions
    {
        public static ILoggingBuilder AddFile(this ILoggingBuilder @this, Action<FileLoggerOptions> configure = null)
        {
            @this.AddConfiguration();
            @this.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();

            // Bind the "Logging:File" configuration section (matched via [ProviderAlias("File")])
            // to FileLoggerOptions, so settings can be supplied from appsettings.json.
            LoggerProviderOptions.RegisterProviderOptions<FileLoggerOptions, FileLoggerProvider>(@this.Services);

            if (configure != null) @this.Services.Configure(configure);
            return @this;
        }

        public static void Error(this ILogger @this, string message)
            => @this.LogError(message);

        /// <summary>
        /// Logs an error, falling back to the exception's own message. Just its message, not its full
        /// dump: the logger writes the exception itself, so a dump here would file every error twice.
        /// </summary>
        public static void Error(this ILogger @this, Exception ex, string message = null)
            => @this.LogError(ex, message.Or(() => ex?.Message));

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
        /// The HttpContext.Items key under which the current request's reference code is stored.
        /// It is set by the reference code middleware, and included in the context of every log
        /// entry written during the request, so that support can find them from the code that the
        /// end user was shown.
        /// </summary>
        public const string ReferenceCodeKey = "Olive.ReferenceCode";

        /// <summary>
        /// When set, provides contextual information (e.g. UserId, RequestUrl, UserIP) to append to log entries.
        /// The default provider returns it as a JSON object, so that consumers such as the audit log
        /// can read the individual properties rather than having to pick them out of free text.
        /// </summary>
        public static Func<string> ContextProvider { get; set; }

        // No 0/O/1/I: users read these codes out to support. Exactly 32 characters, which divides the
        // 256 byte values evenly and so keeps NewReferenceCode unbiased.
        const string ReferenceAlphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

        // One generator, not one per code: a code is minted for every request that reaches the app.
        static readonly RandomNumberGenerator RandomGenerator = RandomNumberGenerator.Create();

        // 32^12 ≈ 1.2e18. At 8 characters the birthday bound makes a collision likely within a million
        // codes, and the audit service stores them under a unique index, so a clash merges two requests.
        const int ReferenceLength = 12;

        static readonly Type HttpContextAccessorType = Type.GetType(
            "Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.Abstractions");

        static readonly PropertyInfo HttpContextProperty = HttpContextAccessorType?.GetProperty("HttpContext");

        // Off the abstract base, so it is resolved once rather than per entry written.
        static readonly PropertyInfo HttpContextItemsProperty = Type.GetType(
            "Microsoft.AspNetCore.Http.HttpContext, Microsoft.AspNetCore.Http.Abstractions")?.GetProperty("Items");

        /// <summary>
        /// The reference code of the work in flight, in an AsyncLocal so that it reaches every log entry
        /// the work writes, however deep.
        /// </summary>
        static readonly AsyncLocal<string> Ambient = new AsyncLocal<string>();

        static readonly AsyncLocal<string> AmbientCause = new AsyncLocal<string>();

        /// <summary>The code of the work that caused the work in flight, or null when nothing did.</summary>
        public static string CurrentCause => AmbientCause.Value;

        /// <summary>
        /// A new reference code of the shape shown to an end user when a request fails:
        /// <c>REF-</c> followed by 12 characters, so sixteen in all.
        /// </summary>
        public static string NewReferenceCode()
        {
            var bytes = new byte[ReferenceLength];
            RandomGenerator.GetBytes(bytes);

            var result = new StringBuilder("REF-", 4 + ReferenceLength);

            foreach (var item in bytes)
                result.Append(ReferenceAlphabet[item % ReferenceAlphabet.Length]);

            return result.ToString();
        }

        /// <summary>
        /// The reference code of the work in flight, or null when there is none. A nested scope wins, so
        /// a command processed inside an HTTP request reports the code of the *user's* request.
        /// HttpContext.Items is only a fallback, for an app that sets a code without the middleware.
        /// </summary>
        public static string CurrentReference => Ambient.Value ?? GetHttpReference();

        /// <summary>
        /// Runs a unit of work under a reference code, so that everything it logs can be found together.
        /// Pass the code the work arrived with and its logs join the work that caused it; pass null and
        /// a fresh code is minted.
        /// </summary>
        public static IDisposable UseReference(string code) => UseReference(code, causedBy: null);

        /// <summary>
        /// Runs work under a reference code of its own, recording the code of the work that caused it.
        /// For work caused by a request but not part of it, where adopting the causing code would spread
        /// it across every service the work reaches. See DestinationEndPoint.ImportUnderOwnReference.
        /// </summary>
        public static IDisposable UseReference(string code, string causedBy)
        {
            var scope = new ReferenceScope();

            // A method group, not a call: only mint a code when one is actually needed.
            Ambient.Value = code.Or(NewReferenceCode);
            AmbientCause.Value = causedBy;

            return scope;
        }

        class ReferenceScope : IDisposable
        {
            // Initialised at construction, i.e. before UseReference overwrites them.
            readonly string Previous = Ambient.Value, PreviousCause = AmbientCause.Value;

            public void Dispose()
            {
                Ambient.Value = Previous;
                AmbientCause.Value = PreviousCause;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Init(Action<IConfigurationBuilder> configurationConfigurator = null, Action<ILoggingBuilder> loggingConfigurator = null)
        {
            if (Factory != null) return;

            var configuration = Config.Build(configurationConfigurator);

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                loggingConfigurator?.Invoke(builder);
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
            ContextProvider = () =>
            {
                try
                {
                    var context = new Dictionary<string, string>();

                    // Null for anything that is not an HTTP request. Such work still has a reference
                    // code, and that alone is a context worth sending.
                    var httpContext = GetHttpContext();
                    var httpReference = httpContext == null ? null : GetHttpReference(httpContext);

                    var reference = Ambient.Value ?? httpReference;

                    // Only when this IS the request. Work nested inside one under a code of its own — a
                    // command drained by /olive/process-command — must not be recorded against its url.
                    if (httpContext != null && reference == httpReference)
                        AddHttpProperties(httpContext, context);

                    if (reference.HasValue()) context["Reference"] = reference;

                    var cause = AmbientCause.Value;
                    if (cause.HasValue()) context["CausedBy"] = cause;

                    if (context.None()) return null;

                    return JsonConvert.SerializeObject(context, Formatting.Indented);
                }
                catch { return null; }
            };
        }

        static object GetHttpContext()
        {
            if (HttpContextAccessorType == null) return null;

            var accessor = Context.Current.GetOptionalService(HttpContextAccessorType);
            if (accessor == null) return null;

            return HttpContextProperty?.GetValue(accessor);
        }

        /// <summary>The reference code the middleware gave the current request, if there is one.</summary>
        static string GetHttpReference(object httpContext = null)
        {
            httpContext = httpContext ?? TryGet(GetHttpContext);
            if (httpContext == null) return null;

            return TryGet(() =>
            {
                var property = HttpContextItemsProperty ?? httpContext.GetType().GetProperty("Items");

                // Cast, not reflection over the indexer: HttpContext.Items' runtime type implements
                // IDictionary<object, object> explicitly, so GetProperty("Item") finds nothing.
                var items = property?.GetValue(httpContext) as IDictionary<object, object>;

                if (items == null) return null;

                return items.TryGetValue(ReferenceCodeKey, out var result) ? result?.ToString() : null;
            });
        }

        static void AddHttpProperties(object httpContext, Dictionary<string, string> context)
        {
            var contextType = httpContext.GetType();

            // User info
            var user = contextType.GetProperty("User")?.GetValue(httpContext) as ClaimsPrincipal;
            var userId = user?.GetId();
            var userEmail = TryGet(() => user?.GetEmail());
            var userRoles = TryGet(() => user?.GetRoles().ToString(", "));

            // Request info
            var request = contextType.GetProperty("Request")?.GetValue(httpContext);
            string requestUrl = null, httpMethod = null, userAgent = null;
            if (request != null)
            {
                var reqType = request.GetType();
                var pathBase = reqType.GetProperty("PathBase")?.GetValue(request)?.ToString();
                var path = reqType.GetProperty("Path")?.GetValue(request)?.ToString();
                var queryString = reqType.GetProperty("QueryString")?.GetValue(request)?.ToString();
                requestUrl = $"{pathBase}{path}{queryString}";
                httpMethod = TryGet(() => reqType.GetProperty("Method")?.GetValue(request)?.ToString());
                userAgent = TryGet(() =>
                {
                    var headers = reqType.GetProperty("Headers")?.GetValue(request);
                    if (headers == null) return null;
                    var indexer = headers.GetType().GetProperty("Item", new[] { typeof(string) });
                    return indexer?.GetValue(headers, new object[] { "User-Agent" })?.ToString();
                });
            }

            // Connection info
            var connection = contextType.GetProperty("Connection")?.GetValue(httpContext);
            var userIp = connection?.GetType().GetProperty("RemoteIpAddress")?.GetValue(connection)?.ToString();

            // Trace identifier
            var traceId = TryGet(() => contextType.GetProperty("TraceIdentifier")?.GetValue(httpContext)?.ToString());

            if (userId.HasValue()) context["UserId"] = userId;
            if (userEmail.HasValue()) context["UserEmail"] = userEmail;
            if (userRoles.HasValue()) context["UserRoles"] = userRoles;
            if (httpMethod.HasValue()) context["HttpMethod"] = httpMethod;
            if (requestUrl.HasValue()) context["RequestUrl"] = requestUrl;
            if (userIp.HasValue()) context["UserIP"] = userIp;
            if (userAgent.HasValue()) context["UserAgent"] = userAgent;
            if (traceId.HasValue()) context["TraceId"] = traceId;
        }

        static T TryGet<T>(Func<T> getter)
        {
            try { return getter(); }
            catch { return default; }
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