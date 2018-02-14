using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Olive
{
    partial class OliveWebExtensions
    {
        public static HttpContext Http(this Context context)
            => context.GetService<IHttpContextAccessor>()?.HttpContext;

        public static ActionContext ActionContext(this Context context)
            => context.GetService<IActionContextAccessor>()?.ActionContext;

        public static HttpRequest Request(this Context context) => context.Http()?.Request;
        public static HttpResponse Response(this Context context) => context.Http()?.Response;
    }
}