using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Olive.Entities;

namespace Olive.Mvc
{
    // TODO: Make it flexible, to be overriden in projects. Use DI

    public class FileUploadService
    {
        public async Task<object> TempSaveUploadedFile(IFormFile file)
        {
            var id = Guid.NewGuid().ToString();

            var path = GetFolder(id).EnsureExists().GetFile(file.FileName.ToSafeFileName());

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                await File.WriteAllBytesAsync(path.FullName, stream.ToArray());
            }

            return new { ID = id, Name = file.FileName.ToSafeFileName() };
        }

        public static DirectoryInfo GetFolder(string key = null)
            => AppDomain.CurrentDomain.WebsiteRoot().GetOrCreateSubDirectory("@Temp.File.Uploads" +
                key.WithPrefix("\\"));

        internal async Task<Blob> Bind(string fileKey)
        {
            if (!fileKey.StartsWith("file:")) throw new Exception("Expected file input is in the format of 'file:{GUID}'.");

            fileKey = fileKey.TrimStart("file:");

            var folder = GetFolder(fileKey);
            if (!folder.Exists())
                throw new Exception("The folder for this uploaded file does not exist: " + fileKey);

            if (folder.GetFiles().None())
                throw new Exception("There is no file in the temp folder " + fileKey);

            if (folder.GetFiles().HasMany())
                throw new Exception("There are multiple files in the temp folder " + fileKey);

            var file = folder.GetFiles().Single();

            return new Blob(await file.ReadAllBytesAsync(), file.Name);
        }

        public static async Task DeleteTempFiles(TimeSpan olderThan)
        {
            foreach (var folder in GetFolder().EnsureExists().GetDirectories())
            {
                // Is it Guid?
                if (folder.Name.TryParseAs<Guid>() == null) continue;

                // Age:
                var age = LocalTime.Now.Subtract(folder.LastWriteTime);
                if (age < olderThan) continue;

                await folder.Delete(recursive: true, harshly: true);
            }
        }
    }
}