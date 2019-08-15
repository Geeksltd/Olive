using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Olive.Mvc.Microservices
{
    public class MaintainCorsHeader
    {
        //this issue has been mentioned here: https://github.com/aspnet/AspNetCore/issues/2378
        //it seems this issue has been fixed in the .net core 2.2 but we are using 2.1.5 right now.

        readonly RequestDelegate next;

        public MaintainCorsHeader(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // Find and hold onto any CORS related headers ...
            var corsHeaders = new HeaderDictionary();
            foreach (var pair in httpContext.Response.Headers)
            {
                if (!pair.Key.ToLower().StartsWith("access-control-", StringComparison.OrdinalIgnoreCase)) { continue; } // Not CORS related
                corsHeaders[pair.Key] = pair.Value;
            }

            // Bind to the OnStarting event so that we can make sure these CORS headers are still included going to the client
            httpContext.Response.OnStarting(o =>
            {
                var ctx = (HttpContext)o;
                var headers = ctx.Response.Headers;
                // Ensure all CORS headers remain or else add them back in ...
                foreach (var pair in corsHeaders)
                {
                    if (headers.ContainsKey(pair.Key)) { continue; } // Still there!
                    headers.Add(pair.Key, pair.Value);
                }

                return Task.CompletedTask;
            }, httpContext);

            // Call the pipeline ...
            await next(httpContext);
        }
    }
}