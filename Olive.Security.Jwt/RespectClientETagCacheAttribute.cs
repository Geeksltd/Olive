namespace Olive.Mvc
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;

    /// <summary>
    /// When applied on a controller or action, it will become aware of client eTag headers in GET requests
    /// and returns empty response with Http Status Code of NotModified instead of the actual response.
    /// </summary>
    public class RespectClientETagCacheAttribute : ActionFilterAttribute
    {
        string[] ClientETags;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ClientETags = (context.HttpContext.Request.Headers.IfNoneMatch?.Select(t => t.Tag.Trim('"')))
                .Trim().ToArray();
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.HttpContext.Request.Method != HttpMethod.Get.ToString()) return;

            var content = context.HttpContext.Response.Content as ObjectContent;
            if (content == null) return;

            if (ClientETags.Contains(Hash(content.Value)))
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotModified;
                context.HttpContext.Response.Content = null;
            }
        }

        static string Hash(object instance)
        {
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                using (var stream = new MemoryStream())
                {
                    new DataContractSerializer(instance.GetType()).WriteObject(stream, instance);
                    sha1.ComputeHash(stream.ToArray());

                    return sha1.Hash.Select(c => c.ToString("x2")).ToString("");
                }
            }
        }
    }
}