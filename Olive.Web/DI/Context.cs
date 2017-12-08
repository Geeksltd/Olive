using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Olive.Web
{
    public static class Context
    {
        static bool isInitialized;

        static IApplicationBuilder applicationBuilder;

        static IHttpContextAccessor httpContextAccessor;

        static IActionContextAccessor actionContextAccessor;

        static IHostingEnvironment hostingEnvironment;

        public static void Initialize(
            IApplicationBuilder applicationBuilder,
            IHostingEnvironment hostingEnvironment,
            IHttpContextAccessor httpContextAccessor,
            IActionContextAccessor actionContextAccessor)
        {
            Context.applicationBuilder = applicationBuilder;
            Context.hostingEnvironment = hostingEnvironment;
            Context.httpContextAccessor = httpContextAccessor;
            Context.actionContextAccessor = actionContextAccessor;

            isInitialized = true;
        }

        public static IApplicationBuilder ApplicationBuilder => applicationBuilder ?? throw GetNotInitializedException();

        public static IHostingEnvironment HostingEnvironment => hostingEnvironment ?? throw GetNotInitializedException();

        public static IHttpContextAccessor HttpContextAccessor => httpContextAccessor ?? throw GetNotInitializedException();

        public static IActionContextAccessor ActionContextAccessor => actionContextAccessor ?? throw GetNotInitializedException();

        public static HttpContext Http => HttpContextAccessor.HttpContext;

        public static HttpRequest Request => Http?.Request;

        public static HttpResponse Response => Http?.Response;

        public static ClaimsPrincipal User => Http.User;

        static Exception GetNotInitializedException() =>
            new InvalidOperationException("HttpContextAccessorHelper is not initialized");

        public static bool IsInitialized => isInitialized;
    }
}
