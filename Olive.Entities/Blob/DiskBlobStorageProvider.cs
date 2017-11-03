using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Entities
{
    class DiskBlobStorageProvider : IBlobStorageProvider
    {
        // TODO: It is a quick workaround for Intern which seems to be back in .Net Core 2
        ConcurrentDictionary<string, AsyncLock> StringKeyAsyncLock = new ConcurrentDictionary<string, AsyncLock>();

        AsyncLock GetAsyncLock(string key) => StringKeyAsyncLock.GetOrAdd(key, x => new AsyncLock());

        public async Task Save(Blob blob)
        {
            var fileDataToSave = await blob.GetFileData(); // Because file data will be lost in delete.

            if (File.Exists(blob.LocalPath))
            {
                using (await GetAsyncLock(blob.LocalPath).Lock())
                {
                    var data = await File.ReadAllBytesAsync(blob.LocalPath);
                    if (data == null) await Delete(blob);
                    else if (data.SequenceEqual(await blob.GetFileData())) return; // Nothing changed.
                    else await Delete(blob);
                }
            }

            using (await GetAsyncLock(blob.LocalPath).Lock())
            {
                await new Func<Task>(async () => await File.WriteAllBytesAsync(blob.LocalPath, fileDataToSave)).Invoke(retries: 6, waitBeforeRetries: TimeSpan.FromSeconds(0.5));
            }
        }

        public async Task Delete(Blob blob)
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

        public async Task<byte[]> Load(Blob blob)
        {
            using (await GetAsyncLock(blob.LocalPath).Lock())
            {
                if (File.Exists(blob.LocalPath))
                    return await File.ReadAllBytesAsync(blob.LocalPath);
            }

            // Look in fall-back paths for file
            foreach (var fallbackPath in blob.FallbackPaths)
            {
                using (await GetAsyncLock(blob.LocalPath).Lock())
                {
                    if (File.Exists(fallbackPath))
                        return await File.ReadAllBytesAsync(fallbackPath);
                }
            }

            return new byte[0];
        }

        public bool FileExists(Blob blob)
        {
            if (blob.LocalPath.HasValue() && File.Exists(blob.LocalPath))
                return true;

            // Check for file in fall-back paths
            if (blob.FallbackPaths.Any(File.Exists))
                return true;

            return false;
        }
    }
}