namespace Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Olive;
    using Olive.Entities;
    using Olive.Mvc;
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public class SharedActionsController : BaseController
    {
        [Route("healthcheck")]
        public async Task<ActionResult> HealthCheck()
        {
            var myIps = Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .Where(x => x.AddressFamily == AddressFamily.InterNetwork)
                .Select(x => x.ToString()).ToString(" | ");

            return Content($"Health check @ {LocalTime.Now.ToLongTimeString()}, " +
                $" version = {Config.Get("App.Resource.Version")} in env:{Context.Current.Environment().EnvironmentName}," +
                $" local IP: {myIps}" + Environment.NewLine +
                $" email: {User.GetEmail()}");
        }

        [Route("refresh-users")]
        public async Task<ActionResult> RefreshUsers()
        {
            await PeopleService.HubEndPoint.UserInfo.RefreshData();
            return Content("Refresh requested");
        }

        [Route("error")]
        public async Task<ActionResult> Error() => View("error");

        [Route("error/404")]
        public new async Task<ActionResult> NotFound() => View("error-404");

        [HttpPost, Route("upload")]
        [Authorize]
        public async Task<ActionResult> UploadTempFileToServer(IFormFile[] files)
        {
            // Note: This will prevent uploading of all unsafe files defined at Blob.UnsafeExtensions
            // If you need to allow them, then comment it out.
            if (Blob.HasUnsafeFileExtension(files[0].FileName))
                return Json(new { Error = "Invalid file extension." });

            // var file = Request.Files[0];
            var path = System.IO.Path.Combine(FileUploadService.GetFolder(Guid.NewGuid().ToString()).FullName, files[0].FileName.ToSafeFileName());
            if (path.Length >= 260)
                return Json(new { Error = "File name length is too long." });

            var result = await new FileUploadService().TempSaveUploadedFile(files[0]);

            return Json(result);
        }

        [Route("file")]
        public async Task<ActionResult> DownloadFile()
        {
            var path = Request.QueryString.ToString().TrimStart('?');
            var accessor = await FileAccessor.Create(path, User);
            if (!accessor.IsAllowed()) return new UnauthorizedResult();

            if (accessor.Blob.IsMedia())
                return await RangeFileContentResult.From(accessor.Blob);
            else return await File(accessor.Blob);
        }

        [Route("temp-file/{key}")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Task<ActionResult> DownloadTempFile(string key)
        {
            return TempFileService.Download(key);
        }
    }
}