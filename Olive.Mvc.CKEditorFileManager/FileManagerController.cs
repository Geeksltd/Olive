using Microsoft.AspNetCore.Mvc;
using Olive.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Mvc.CKEditorFileManager
{
    public class FileManagerController : Controller
    {
        // [Authorize]
        [Route("ckeditorfileupload")]
        public async Task<IActionResult> Upload()
        {
            try
            {
                if (!Request.HasFormContentType)
                    throw new Exception("The request does not contain a file.");

                foreach (var file in Request.Form.Files)
                {
                    using (var stream = file.OpenReadStream())
                    {
                        var ckeFile = (await FindByFileName(file.FileName))?.Clone() as ICKEditorFile
                            ?? CreateNewFileInstance();

                        ckeFile.File = new Blob(await stream.ReadAllBytesAsync(), file.FileName);

                        await Database.Save(ckeFile);
                    }
                }

                return Content("Done");
            }
            catch (Exception exception)
            {
                return Content(exception.Message);
            }
        }

        // [Authorize]
        [Route("ckeditorfilebrowser")]
        public async Task<IActionResult> Browser()
        {
            var files = await Database.GetList<ICKEditorFile>();
            
            var model = new ViewModel
            {
                Files = files.Select(f => new DownloadableFileDto
                {
                    CKEditorFile = f,
                    Uri = $"/ckeditorfiledownload/{Uri.EscapeUriString(f.File.FileName)}"
                }).ToArray()
            };

            return View("CKEditorFileBrowser", model);
        }

        [Route("ckeditorfiledownload/{filename}")]
        public async Task<IActionResult> Browser(string filename) => await File((await FindByFileName(filename)).File);

        async Task<ICKEditorFile> FindByFileName(string filename) =>
            (await Database.GetList<ICKEditorFile>()).FirstOrDefault(x => x.File.FileName == filename);

        ICKEditorFile CreateNewFileInstance()
        {
            var type = AppDomain.CurrentDomain.FindImplementers(typeof(ICKEditorFile)).FirstOrDefault();

            if (type == null)
                throw new Exception($"No implementer found for interface '{nameof(ICKEditorFile)}'");

            return Activator.CreateInstance(type) as ICKEditorFile;
        }
    }
}