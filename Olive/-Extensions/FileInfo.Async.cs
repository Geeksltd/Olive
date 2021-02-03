using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveExtensions
    {
        static readonly ConcurrentDictionary<string, AsyncLock> FileSyncLocks = new ConcurrentDictionary<string, AsyncLock>();
        public static AsyncLock GetSyncLock(this FileInfo file)
        {
            return FileSyncLocks.GetOrAdd(file.FullName.ToLower(), f => new AsyncLock());
        }

        /// <summary>
        /// Copies this file onto the specified destination path.
        /// </summary>
        public static async Task CopyToAsync(this FileInfo file, FileInfo destinationPath, bool overwrite = true)
        {
            using (await destinationPath.GetSyncLock().Lock())
            using (await file.GetSyncLock().Lock())
            {
                if (!overwrite && destinationPath.Exists()) return;

                var content = await DoReadAllBytesAsync(file.ExistsOrThrow());
                await DoWriteAllBytesAsync(destinationPath, content);
            }
        }

        /// <summary>
        /// Gets the entire content of this file.
        /// If the file does not exist, it will return an empty byte array.
        /// </summary>
        public static async Task<byte[]> ReadAllBytesAsync(this FileInfo @this)
        {
            using (await @this.GetSyncLock().Lock())
                return await DoReadAllBytesAsync(@this);
        }

        static async Task<byte[]> DoReadAllBytesAsync(FileInfo file)
        {
            if (!File.Exists(file.FullName)) return new byte[0];
            byte[] result;
            using var stream = File.Open(file.FullName, FileMode.Open);
            result = new byte[stream.Length];
            await stream.ReadAsync(result, 0, (int)stream.Length);
            return result;
        }

        /// <summary>
        /// Returns whether the file exists. If it's null, false will be returned.
        /// It awaits any other concurrent file operations before checking the file's existence.
        /// </summary>
        public static async Task<bool> ExistsAsync(this FileInfo @this)
        {
            if (@this == null) return false;
            using (await @this.GetSyncLock().Lock())
                return File.Exists(@this.FullName);
        }

        /// <summary>
        /// Gets the entire content of this file.
        /// </summary>
        public static Task<string> ReadAllTextAsync(this FileInfo @this) => ReadAllTextAsync(@this, DefaultEncoding);

        /// <summary>
        /// Gets the entire content of this file.
        /// </summary>
        public static async Task<string> ReadAllTextAsync(this FileInfo @this, Encoding encoding)
        {
            using (await @this.GetSyncLock().Lock())
            using (var stream = new FileStream(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream, encoding))
                return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// Will try to delete a specified directory by first deleting its sub-folders and files.
        /// </summary>
        /// <param name="harshly">If set to true, then it will try multiple times, in case the file is temporarily locked.</param>
        public static async Task DeleteAsync(this FileInfo @this, bool harshly)
        {
            if (@this == null) return;

            using (await @this.GetSyncLock().Lock())
            {
                var retry = 0;
                while (true)
                {
                    try
                    {
                        if (File.Exists(@this.FullName))
                        {
                            File.Delete(@this.FullName);
                            return;
                        }
                    }
                    catch
                    {
                        if (!harshly) throw;

                        retry++;
                        if (retry > 3) throw;
                        await Task.Delay((int)Math.Pow(10, retry));
                    }
                }
            }
        }

        /// <summary>
        /// Saves the specified content on this file.
        /// </summary>
        public static async Task WriteAllBytesAsync(this FileInfo @this, byte[] content)
        {
            using (await @this.GetSyncLock().Lock())
                await DoWriteAllBytesAsync(@this, content);
        }

        static async Task DoWriteAllBytesAsync(FileInfo file, byte[] content)
        {
            file.Directory.EnsureExists();

            using var stream = new FileStream(file.FullName,
                FileMode.Create, FileAccess.Write, FileShare.None,
                0x4096, useAsync: true);
            await stream.WriteAsync(content, 0, content.Length);
        }

        /// <summary>
        /// Saves the specified content on this file using the Western European Windows Encoding 1252.
        /// </summary>
        public static Task WriteAllTextAsync(this FileInfo @this, string content)
            => WriteAllTextAsync(@this, content, DefaultEncoding);

        /// <summary>
        /// Saves the specified content on this file. 
        /// Note: For backward compatibility, for UTF-8 encoding, it will always add the BOM signature.
        /// </summary>
        public static Task WriteAllTextAsync(this FileInfo @this, string content, Encoding encoding)
        {
            if (encoding == null) encoding = DefaultEncoding;
            if (encoding is UTF8Encoding) encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

            var data = encoding.GetBytes(content);
            return @this.WriteAllBytesAsync(data);
        }
    }
}