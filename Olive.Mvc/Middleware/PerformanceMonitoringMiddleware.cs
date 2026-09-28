using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Olive.Mvc
{
    class PerformanceMonitoringMiddleware : BaseMiddleware
    {
        static HashSet<string> Whitelist = new HashSet<string>();

        const int PERFROMANCE_THRESHOLD_SECONDS = 3;
        public PerformanceMonitoringMiddleware(RequestDelegate next) : base(next)
        {
        }

        public override async Task Invoke(HttpContext context)
        {
            var start = LocalTime.Now;

            await Next.Invoke(context);

            var length = LocalTime.Now.Subtract(start).TotalSeconds;

            if (context.Request.Path.HasValue == false) return;

            var url = context.Request.Path.Value.ToLower();
            var user = context.User?.GetId();

            // A slow request still succeeded, so it is a warning rather than an error. A whitelisted url
            // is known to be slow, so it is only worth seeing when diagnosing.
            if (length > Config.Get("PerformanceThresholdSeconds", PERFROMANCE_THRESHOLD_SECONDS)) // slow
                if (!Whitelist.Contains(url))
                {
                    var slow = new UnacceptablePerformanceException($"Slow action ({length}) seconds url :> " + url + user.WithPrefix(" for user : "));
                    Log.For(this).Warning(slow);
                }
                else
                    Log.For(this)
                        .Debug($"UnacceptablePerformance ({length}) seconds for whitelisted url : " + url + user.WithPrefix(" for user : "));
        }

        internal static void IgnorePerformance(string url) => Whitelist.Add(url.ToLower());

        class UnacceptablePerformanceException : Exception
        {
            public UnacceptablePerformanceException(string description) : base(description)
            {
            }
        }
    }

    public static class PerformanceMonitoringMiddlewareExtensions
    {
        public static IApplicationBuilder IgnorePerformance(this IApplicationBuilder app, string url)
        {
            PerformanceMonitoringMiddleware.IgnorePerformance(url);
            return app;
        }
    }
}