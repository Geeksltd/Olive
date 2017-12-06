namespace Olive.Mvc
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Primitives;
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
            ClientETags = (GetIfNoneMatch(context.HttpContext.Request.Headers)?.Select(t => t.Trim('"')))
                .Trim().ToArray();
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.HttpContext.Request.Method != HttpMethod.Get.ToString()) return;

            var content = context.Result;
            if (content == null) return;

            if (ClientETags.Contains(Hash(content)))
                context.Result = new StatusCodeResult((int)HttpStatusCode.NotModified);
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

        StringValues? GetIfNoneMatch(IHeaderDictionary headers) 
            => headers.Keys.Contains("If-None-Match") ? headers["If-None-Match"] : (StringValues?)null; // HttpRequestHeader.IfNoneMatch
    }
}