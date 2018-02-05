using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Olive.Entities.Blob;

namespace Olive.Entities
{
    public class DiskBlobStorageProvider : IBlobStorageProvider
    {
        static DirectoryInfo root;

        /// <summary>
        /// Gets the physical path root.
        /// </summary>
        public static DirectoryInfo Root => root ?? (root = GetRoot(AppDomain.CurrentDomain.WebsiteRoot()));

        public static DirectoryInfo GetRoot(DirectoryInfo baseAddress)
        {
            var folder = Config.Get("Blob:RootPath", "Blob");

            if (!folder.StartsWith("\\\\") && folder[1] != ':') // Relative address:
                folder = baseAddress.GetSubDirectory(folder).FullName;

            return folder.AsDirectory();
        }

        // TODO: It is a quick workaround for Intern which seems to be back in .Net Core 2
        protected readonly ConcurrentDictionary<string, AsyncLock> StringKeyAsyncLock = new ConcurrentDictionary<string, AsyncLock>();

        protected virtual AsyncLock GetAsyncLock(string key) => StringKeyAsyncLock.GetOrAdd(key, x => new AsyncLock());

        static FileInfo File(Blob blob)
        {
            if (blob.ownerEntity == null) return null;
            var folder = Folder(blob);
            return folder.GetFile(blob.OwnerId() + blob.FileExtension);
        }

        static DirectoryInfo Folder(Blob blob)
        {
            if (blob.ownerEntity == null) return null;
            return Root.GetOrCreateSubDirectory(blob.FolderName);
        }

        public virtual async Task SaveAsync(Blob blob)
        {
            var path = File(blob);
            if (path == null) throw new InvalidOperationException("This blob is not linked to any entity.");

            var fileDataToSave = await blob.GetFileDataAsync(); // Because file data will be lost in delete.

            if (path.Exists())
            {
                using (await GetAsyncLock(path.FullName).Lock())
                {
                    var data = await path.ReadAllBytesAsync();
                    if (data == null) await DeleteAsync(blob);
                    else if (data.SequenceEqual(await blob.GetFileDataAsync())) return; // Nothing changed.
                    else await DeleteAsync(blob);
                }
            }

            using (await GetAsyncLock(path.FullName).Lock())
            {
                await path.WriteAllBytesAsync(fileDataToSave);
            }
        }

        public virtual async Task DeleteAsync(Blob blob)
        {
            var folder = Folder(blob);

            var tasks = new List<Task>();

            // Delete old file. TODO: Archive the files instead of deleting.
            foreach (var file in folder.GetFiles(blob.OwnerId() + ".*"))
            {
                using (await GetAsyncLock(file.FullName).Lock())
                {
                    tasks.Add(new Func<Task>(async () => await Task.Factory.StartNew(() => file.Delete(harshly: true)))
                        .Invoke(retries: 6, waitBeforeRetries: TimeSpan.FromSeconds(0.5)));
                }
            }

            await Task.WhenAll(tasks);
        }

        public virtual async Task<byte[]> LoadAsync(Blob blob)
        {
            var path = File(blob);
            if (path == null) return new byte[0];

            using (await GetAsyncLock(path.FullName).Lock())
            {
                if (path.Exists())
                    return await path.ReadAllBytesAsync();
            }

            return new byte[0];
        }

        public virtual Task<bool> FileExistsAsync(Blob blob) => Task.FromResult(File(blob)?.Exists() == true);

        public virtual bool CostsToCheckExistence() => false;
    }
}