using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public class TempFileService
    {
        public static async Task<ActionResult> Download(string key)
        {
            var folder = FileUploadService.GetFolder(key);
            if (!folder.Exists()) return CreateError("The folder does not exist for key: " + key);

            var files = folder.GetFiles();

            if (files.None()) return CreateError("There is no file for key: " + key);
            if (files.HasMany()) return CreateError("There are multiple files for the key: " + key);

            var file = files.Single();

            return new FileContentResult(await file.ReadAllBytesAsync(), "application/octet-stream") { FileDownloadName = file.Name };
        }

        static FileContentResult CreateError(string errorText)
        {
            var bytes = Encoding.ASCII.GetBytes(errorText);

            return new FileContentResult(bytes, "application/octet-stream") { FileDownloadName = "Error.txt" };
        }

        public static async Task<object> CreateDownloadAction(byte[] data, string filename)
        {
            var key = Guid.NewGuid().ToString();
            var folder = FileUploadService.GetFolder(key).EnsureExists();
            await folder.GetFile(filename).WriteAllBytesAsync(data);

            var url = "/temp-file/" + key;

            var http = Context.Current.Http();
            if (http != null)
                url = http?.GetUrlHelper().Content("~" + url);

            return new { Download = url };
        }
    }
}