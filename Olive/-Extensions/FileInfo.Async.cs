using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveExtensions
    {
        static ConcurrentDictionary<string, AsyncLock> FileSyncLocks = new ConcurrentDictionary<string, AsyncLock>();
        public static AsyncLock GetSyncLock(this FileInfo file)
        {
            return FileSyncLocks.GetOrAdd(file.FullName.ToLower(), f => new AsyncLock());
        }

        /// <summary>
        /// Copies this file onto the specified desination path.
        /// </summary>
        public static async Task CopyToAsync(this FileInfo file, FileInfo destinationPath, bool overwrite = true)
        {
            using (await file.GetSyncLock().Lock())
            {
                if (!overwrite && destinationPath.Exists()) return;
                if (!file.Exists()) throw new Exception("File does not exist: " + file.FullName);

                var content = await file.ReadAllBytesAsync();
                await destinationPath.WriteAllBytesAsync(content);
            }
        }

        /// <summary>
        /// Gets the entire content of this file.
        /// </summary>
        public static async Task<byte[]> ReadAllBytesAsync(this FileInfo file)
        {
            using (await file.GetSyncLock().Lock())
            {
                byte[] result;
                using (var stream = File.Open(file.FullName, FileMode.Open))
                {
                    result = new byte[stream.Length];
                    await stream.ReadAsync(result, 0, (int)stream.Length);
                    return result;
                }
            }
        }

        /// <summary>
        /// Gets the entire content of this file.
        /// </summary>
        public static Task<string> ReadAllTextAsync(this FileInfo file) => ReadAllTextAsync(file, DefaultEncoding);

        /// <summary>
        /// Gets the entire content of this file.
        /// </summary>
        public static async Task<string> ReadAllTextAsync(this FileInfo file, Encoding encoding)
        {
            using (await file.GetSyncLock().Lock())
            using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream, encoding))
                return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// Will try to delete a specified directory by first deleting its sub-folders and files.
        /// </summary>
        /// <param name="harshly">If set to true, then it will try multiple times, in case the file is temporarily locked.</param>
        public static async Task DeleteAsync(this FileInfo file, bool harshly)
        {
            if (!file.Exists()) return;

            using (await file.GetSyncLock().Lock())
                await file.DeleteAsync(harshly);
        }

        /// <summary>
        /// Saves the specified content on this file.
        /// </summary>
        public static async Task WriteAllBytesAsync(this FileInfo file, byte[] content)
        {
            if (!file.Directory.Exists()) file.Directory.Create();

            using (await file.GetSyncLock().Lock())
                await DoTryHardAsync(file, () => Task.Run(() => File.WriteAllBytes(file.FullName, content)),
                "The system cannot write the specified content on the file: {0}");
        }

        /// <summary>
        /// Saves the specified content on this file using the Western European Windows Encoding 1252.
        /// </summary>
        public static Task WriteAllTextAsync(this FileInfo file, string content)
            => WriteAllTextAsync(file, content, DefaultEncoding);

        /// <summary>
        /// Saves the specified content on this file. 
        /// Note: For backward compatibility, for UTF-8 encoding, it will always add the BOM signature.
        /// </summary>
        public static async Task WriteAllTextAsync(this FileInfo file, string content, Encoding encoding)
        {
            if (encoding == null) encoding = DefaultEncoding;
            file.Directory.EnsureExists();
            if (encoding is UTF8Encoding) encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

            using (await file.GetSyncLock().Lock())
                await Task.Run(() => File.WriteAllText(file.FullName, content, encoding));
        }

        /// <summary>
        /// Saves the specified content to the end of this file.
        /// </summary>
        public static Task AppendAllTextAsync(this FileInfo file, string content)
            => AppendAllTextAsync(file, content, DefaultEncoding);

        /// <summary>
        /// Saves the specified content to the end of this file.
        /// </summary>
        public static Task AppendLineAsync(this FileInfo file, string content = null)
            => AppendAllTextAsync(file, content + Environment.NewLine, DefaultEncoding);

        /// <summary>
        /// Saves the specified content to the end of this file.
        /// </summary>
        public static async Task AppendAllTextAsync(this FileInfo file, string content, Encoding encoding)
        {
            if (encoding == null) encoding = DefaultEncoding;
            file.Directory.EnsureExists();

            using (await file.GetSyncLock().Lock())
                await Task.Run(() => File.AppendAllText(file.FullName, content, encoding));
        }
    }
}