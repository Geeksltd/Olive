using Microsoft.AspNetCore.Http;
using Olive.Entities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    // TODO: Make it flexible, to be overriden in projects. Use DI

    public class FileUploadService
    {
        public static DirectoryInfo GetFolder(string key = null)
        {
            var configuredPath = Config.Get("Blob:TempFileAbsolutePath");

            if(configuredPath.IsEmpty())
                configuredPath = Config.Get("Blob:TempFilePath")
                    .WithPrefix(AppDomain.CurrentDomain.WebsiteRoot().FullName);

            if (configuredPath.HasValue())
                return configuredPath.AsDirectory().GetOrCreateSubDirectory(key).EnsureExists();

            return AppDomain.CurrentDomain.WebsiteRoot()
                .GetOrCreateSubDirectory("@Temp.File.Uploads" + key.WithPrefix("\\"));
        }

        internal async Task<Blob> Bind(string fileKey)
        {
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

                await folder.DeleteAsync(recursive: true, harshly: true);
            }
        }

        /// <param name="allowUnsafeExtension">If set to false (default) it will prevent uploading of all unsafe files (as defined in Blob.GetUnsafeExtensions())</param>
        public async Task<object> TempSaveUploadedFile(IFormFile file, bool allowUnsafeExtension = false)
        {
            if (!allowUnsafeExtension && Blob.HasUnsafeFileExtension(file.FileName))
                return new { Error = "Invalid file extension." };

            var id = Guid.NewGuid().ToString();

            var path = GetFolder(id).EnsureExists().GetFile(file.FileName.ToSafeFileName());
            if (path.FullName.Length >= 260)
                return new { Error = "File name length is too long." };

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                await File.WriteAllBytesAsync(path.FullName, stream.ToArray());
            }

            return new
            {
                Result = new { ID = id, Name = file.FileName.ToSafeFileName() }
            };
        }
    }
}