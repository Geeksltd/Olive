using Microsoft.AspNetCore.Mvc;
using Olive.Entities;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    partial class Controller
    {
        /// <summary>
        /// Gets a FilePathResult based on the file's path. It sets the mime type based on the file's extension.
        /// </summary>
        /// <param name="downloadFileName">If specified, the browser will not try to process the file directly (such as PDF files) and instead always opens the file download dialogue.</param>
        protected async Task<ActionResult> File(Blob file,
            string downloadFileName = null, CacheControlHeaderValue cacheControl = null, bool showInline = false)
        {
            if (cacheControl != null)
            {
                Response.Headers.Remove(Microsoft.Net.Http.Headers.HeaderNames.Pragma);
                Response.Headers.Remove(Microsoft.Net.Http.Headers.HeaderNames.CacheControl);
                Response.Headers.Add(Microsoft.Net.Http.Headers.HeaderNames.CacheControl, cacheControl.ToString());
            }

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = downloadFileName.Or(file.FileName).ToASCII(),
                Inline = showInline,
            };

            if (showInline)
                Response.Headers.Add("X-Content-Type-Options", "nosniff");

            Response.Headers.Remove("Content-Disposition");
            Response.Headers.Add(Microsoft.Net.Http.Headers.HeaderNames.ContentDisposition, cd.ToString());

            return File(await file.GetFileDataAsync(), file.GetMimeType());
        }

        protected async Task<JsonResult> NonobstructiveFile(byte[] data, string filename)
        {
            return AddAction(await Context.Current
                .GetService<IFileRequestService>()
                .CreateDownloadAction(data, filename));
        }

        protected async Task<JsonResult> NonobstructiveFile(Blob file, string downloadFileName = null) =>
            await NonobstructiveFile(await file.GetFileDataAsync(), downloadFileName.Or(file.FileName));

        protected async Task<JsonResult> NonobstructiveFile(FileInfo file) =>
            await NonobstructiveFile(await file.ReadAllBytesAsync(), file.Name);
    }
}