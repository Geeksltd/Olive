namespace Olive.Mvc
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// When applied on a controller or action, it will become aware of client eTag headers in GET requests
    /// and returns empty response with Http Status Code of NotModified instead of the actual response.
    /// Also if the client request header does not have the etag, the first time it's requested it will be returned.
    /// </summary>
    public class SupportETagCacheAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.HttpContext.Request.Method != HttpMethod.Get.ToString()) return;

            var responseBytes = context.HttpContext.Response.Body.ReadAllBytes();

            var responseEtag = CreateEtag(responseBytes);

            if (ExtractEtag(context.HttpContext).Contains(responseEtag))
            {
                context.Result = new StatusCodeResult((int)HttpStatusCode.NotModified);
            }
            else
            {
                // Add etag to the resopnse headers for future requests:
                context.HttpContext.Response.Headers.Add("If-None-Match", new StringValues(responseEtag));
            }
        }

        static string CreateEtag(byte[] response)
        {
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                sha1.ComputeHash(response);
                return sha1.Hash.Select(c => c.ToString("x2")).ToString("");
            }
        }

        string[] ExtractEtag(HttpContext context)
        {
            var headers = context.Request.Headers;
            return headers.GetOrDefault("If-None-Match").Select(t => t.Trim('"')).Trim().ToArray();
        }
    }
}