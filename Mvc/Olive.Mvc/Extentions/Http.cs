using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Olive.Entities;
using Olive.Web;

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
        /// Dispatches a binary data block back to the client.
        /// </summary>
        public static async Task Dispatch(this HttpResponse response, byte[] responseData, string fileName, string contentType = "Application/octet-stream")
        {
            if (responseData == null) throw new ArgumentNullException(nameof(responseData));

            if (fileName.IsEmpty()) throw new ArgumentNullException(nameof(fileName));

            response.Clear();
            response.ContentType = contentType;

            response.Headers.Add("Cache-Control", "no-store");
            response.Headers.Add("Pragma", "no-cache");

            response.Headers.Add("Content-Disposition", "attachment; filename=\"{0}\"".FormatWith(fileName.Remove("\"", ",")));

            await response.Body.WriteAsync(responseData, 0, responseData.Length);
        }

        /// <summary>
        /// Dispatches a file back to the client.
        /// </summary>
        public static async Task Dispatch(this HttpResponse response, Blob blob, string contentType = "Application/octet-stream") =>
            await Dispatch(response, blob.LocalPath.AsFile(), blob.FileName, contentType);

        /// <summary>
        /// Dispatches a file back to the client.
        /// </summary>
        /// <param name="fileName">If set to null, the same file name of the file will be used.</param>
        public static async Task Dispatch(this HttpResponse response, FileInfo responseFile, string fileName = null, string contentType = "Application/octet-stream")
        {
            if (responseFile == null) throw new ArgumentNullException(nameof(responseFile));

            if (fileName.IsEmpty()) fileName = responseFile.Name;

            var data = await responseFile.ReadAllBytesAsync();

            await response.Dispatch(data, fileName, contentType);
        }

        /// <summary>
        /// Dispatches a string back to the client as a file.
        /// </summary>
        public static async Task Dispatch(this HttpResponse response, string responseText, string fileName, string contentType = "Application/octet-stream", System.Text.Encoding encoding = null)
        {
            response.Clear();

            if (encoding == null) encoding = Encoding.UTF8;

            var bytes = encoding == Encoding.UTF8 ? responseText.GetUtf8WithSignatureBytes() : encoding.GetBytes(responseText);

            await response.Dispatch(bytes, fileName, contentType);
        }

        /// <summary>
        /// Gets a URL helper for the current http context.
        /// </summary>
        public static UrlHelper GetUrlHelper(this HttpContext context) =>
            new UrlHelper(Context.ActionContextAccessor.ActionContext);
    }
}