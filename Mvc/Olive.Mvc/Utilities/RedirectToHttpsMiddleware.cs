using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Olive.Mvc
{
    public class RedirectToHttpsMiddleware
    {
        private readonly RequestDelegate Next;
        public RedirectToHttpsMiddleware(RequestDelegate next)
        {
            Next = next;
        }
        public System.Threading.Tasks.Task Invoke(HttpContext context)
        {
            if (!(context.Request.IsHttps || context.Request.Headers.ContainsKey("X-ARR-SSL")))
            {
                var request = context.Request;
                var redirectUrl = $"https://{request.Host}{request.PathBase}{request.Path}{request.QueryString}";
                context.Response.Redirect(redirectUrl, permanent: true);
            }

            return Next(context);
        }
    }
}
