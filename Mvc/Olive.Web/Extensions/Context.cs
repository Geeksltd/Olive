using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Olive
{
    partial class OliveWebExtensions
    {
        static IHostingEnvironment environment;

        public static ClaimsPrincipal User(this Context context)
            => context.Http()?.User;

        public static HttpContext Http(this Context context)
            => context.GetService<IHttpContextAccessor>()?.HttpContext;

        public static ActionContext ActionContext(this Context context)
            => context.GetService<IActionContextAccessor>()?.ActionContext;

        public static HttpRequest Request(this Context context) => context.Http()?.Request;

        public static HttpResponse Response(this Context context) => context.Http()?.Response;

        public static IHostingEnvironment Environment(this Context context)
            => environment
            ?? throw new Exception("Environment is not set via Context.Configure()");

        public static Context Configure(this Context context, IHostingEnvironment env)
        {
            environment = env;
            return context;
        }
    }
}