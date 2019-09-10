using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    partial class OliveMvcExtensions
    {
        public static HttpContext GetHttpContextBase(this HttpContext context)
        {
            var owinInfo = context.Items["owin.Environment"] as
                            IDictionary<string, object>;

            return owinInfo?["System.Web.HttpContextBase"] as HttpContext;
        }

        /// <summary>
        /// Determines if this is an Ajax GET http request.
        /// </summary>
        public static bool IsAjaxGet(this HttpRequest request) => request.IsAjaxCall() && request.IsGet();

        /// <summary>
        /// Determines if this is an Ajax Post http request.
        /// </summary>
        public static bool IsAjaxPost(this HttpRequest request) => request.IsAjaxCall() && request.IsPost();

        /// <summary>
        /// Gets a URL helper for the current http context.
        /// </summary>
        public static UrlHelper GetUrlHelper(this HttpContext context)
            => new UrlHelper(Context.Current.ActionContext());
    }
}