using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Olive.Entities;
using System.IO;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    partial class Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        /// <summary>
        /// Gets a FilePathResult based on the file's path. It sets the mime type based on the file's extension.
        /// </summary>
        /// <param name="downloadFileName">If specified, the browser will not try to process the file directly (such as PDF files) and instead always opens the file download dialogue.</param>
        protected async Task<ActionResult> File(Blob file,
            string downloadFileName = null, CacheControlHeaderValue cacheControl = null)
        {
            if (cacheControl != null)
            {
                Response.Headers.Remove(HeaderNames.Pragma);
                Response.Headers.Remove(HeaderNames.CacheControl);
                Response.Headers.Add(HeaderNames.CacheControl, cacheControl.ToString());
            }

            return File(await file.GetFileDataAsync(), file.GetMimeType(), downloadFileName.Or(file.FileName));
        }

        protected JsonResult NonobstructiveFile(byte[] data, string filename)
          => AddAction(TempFileService.CreateDownloadAction(data, filename));

        protected async Task<JsonResult> NonobstructiveFile(Blob file, string downloadFileName = null) =>
            NonobstructiveFile(await file.GetFileDataAsync(), downloadFileName.Or(file.FileName));

        protected async Task<JsonResult> NonobstructiveFile(FileInfo file) =>
            NonobstructiveFile(await file.ReadAllBytesAsync(), file.Name);
    }
}