using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Security.Claims;

namespace Olive
{
    partial class OliveWebExtensions
    {
        static IWebHostEnvironment environment;

        public static ClaimsPrincipal User(this Context context)
            => context.Http()?.User;

        public static HttpContext Http(this Context context)
            => context.GetService<IHttpContextAccessor>()?.HttpContext;

        public static ActionContext ActionContext(this Context context)
            => context.GetService<IActionContextAccessor>()?.ActionContext;

        public static HttpRequest Request(this Context context) => context.Http()?.Request;

        public static HttpResponse Response(this Context context) => context.Http()?.Response;

        public static IWebHostEnvironment Environment(this Context context)
            => environment ?? (environment = context.GetService<IWebHostEnvironment>());
    }
}