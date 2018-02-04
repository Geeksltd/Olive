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
        static IApplicationBuilder applicationBuilder;
        static IHttpContextAccessor httpContextAccessor;
        static IActionContextAccessor actionContextAccessor;
        static IHostingEnvironment hostingEnvironment;

        public static HttpContext Http => HttpContextAccessor.HttpContext;
        public static HttpRequest Request => Http?.Request;
        public static HttpResponse Response => Http?.Response;
        public static ClaimsPrincipal User => Http.User;
        public static bool IsInitialized { get; private set; }

        public static IApplicationBuilder ApplicationBuilder => Return(applicationBuilder);
        public static IHostingEnvironment HostingEnvironment => Return(hostingEnvironment);
        public static IHttpContextAccessor HttpContextAccessor => Return(httpContextAccessor);
        public static IActionContextAccessor ActionContextAccessor => Return(actionContextAccessor);

        static T Return<T>(T item) where T : class
            => item ?? throw new InvalidOperationException("Olive.Web.Context is not initialized");

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

            Entities.DefaultApplicationEventManager.InitializeUseAccessor(() => User);
            Entities.DefaultApplicationEventManager.InitializeIpAccessor(() => Http.Connection.RemoteIpAddress.ToString());

            IsInitialized = true;
        }
    }
}