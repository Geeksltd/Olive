using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Entities
{
    public class DiskBlobStorageProvider : IBlobStorageProvider
    {
        // TODO: It is a quick workaround for Intern which seems to be back in .Net Core 2
        protected readonly ConcurrentDictionary<string, AsyncLock> StringKeyAsyncLock = new ConcurrentDictionary<string, AsyncLock>();

        protected virtual AsyncLock GetAsyncLock(string key) => StringKeyAsyncLock.GetOrAdd(key, x => new AsyncLock());

        public virtual async Task SaveAsync(Blob blob)
        {
            var fileDataToSave = await blob.GetFileDataAsync(); // Because file data will be lost in delete.

            if (File.Exists(blob.LocalPath))
            {
                using (await GetAsyncLock(blob.LocalPath).Lock())
                {
                    var data = await File.ReadAllBytesAsync(blob.LocalPath);
                    if (data == null) await DeleteAsync(blob);
                    else if (data.SequenceEqual(await blob.GetFileDataAsync())) return; // Nothing changed.
                    else await DeleteAsync(blob);
                }
            }

            using (await GetAsyncLock(blob.LocalPath).Lock())
            {
                await new Func<Task>(async () => await File.WriteAllBytesAsync(blob.LocalPath, fileDataToSave)).Invoke(retries: 6, waitBeforeRetries: TimeSpan.FromSeconds(0.5));
            }
        }

        public virtual async Task DeleteAsync(Blob blob)
        {
            if (!Directory.Exists(blob.LocalFolder)) Directory.CreateDirectory(blob.LocalFolder);

            var tasks = new List<Task>();

            // Delete old file. TODO: Archive the files instead of deleting.
            foreach (var file in Directory.GetFiles(blob.LocalFolder, blob.GetFileNameWithoutExtension() + ".*"))
            {
                using (await GetAsyncLock(file).Lock())
                {
                    tasks.Add(new Func<Task>(async () => await Task.Factory.StartNew(() => File.Delete(file)))
                        .Invoke(retries: 6, waitBeforeRetries: TimeSpan.FromSeconds(0.5))
                        );
                }
            }

            await Task.WhenAll(tasks);
        }

        public virtual async Task<byte[]> LoadAsync(Blob blob)
        {
            using (await GetAsyncLock(blob.LocalPath).Lock())
            {
                if (File.Exists(blob.LocalPath))
                    return await File.ReadAllBytesAsync(blob.LocalPath);
            }

            return new byte[0];
        }

        public virtual Task<bool> FileExistsAsync(Blob blob)
        {
            var exists = blob.LocalPath.HasValue() && File.Exists(blob.LocalPath);
            return Task.FromResult(exists);
        }

        public virtual bool CostsToCheckExistence() => false;
    }
}