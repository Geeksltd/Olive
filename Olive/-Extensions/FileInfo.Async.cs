using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveExtensions
    {
        /// <summary>
        /// Copies this file onto the specified desination path.
        /// </summary>
        public static async Task CopyToAsync(this FileInfo file, FileInfo destinationPath, bool overwrite = true)
        {
            if (!overwrite && destinationPath.Exists()) return;
            if (!file.Exists()) throw new Exception("File does not exist: " + file.FullName);

            var content = await file.ReadAllBytesAsync();
            await destinationPath.WriteAllBytesAsync(content);
        }

        /// <summary>
        /// Gets the entire content of this file.
        /// </summary>
        public static Task<byte[]> ReadAllBytesAsync(this FileInfo file)
        {
            return TryHardAsync(file, () => Task.Run(() => File.ReadAllBytes(file.FullName)),
                "The system cannot read the file: {0}");
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
            Func<Task<string>> readFile = async () =>
            {
                using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var reader = new StreamReader(stream, encoding))
                        return await reader.ReadToEndAsync();
                }
            };

            return await TryHardAsync(file, readFile, "The system cannot read the file: {0}");
        }

        /// <summary>
        /// Will try to delete a specified directory by first deleting its sub-folders and files.
        /// </summary>
        /// <param name="harshly">If set to true, then it will try multiple times, in case the file is temporarily locked.</param>
        public static async Task DeleteAsync(this FileInfo file, bool harshly)
        {
            if (!file.Exists()) return;

            if (!harshly)
            {
                await Task.Factory.StartNew(file.Delete);
                return;
            }

            await DoTryHardAsync(file, () => Task.Run(() => file.DeleteAsync(harshly)),
                "The system cannot delete the file, even after several attempts. Path: {0}");
        }

        /// <summary>
        /// Saves the specified content on this file.
        /// </summary>
        public static async Task WriteAllBytesAsync(this FileInfo file, byte[] content)
        {
            if (!file.Directory.Exists())
                file.Directory.Create();

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
        public static Task WriteAllTextAsync(this FileInfo file, string content, Encoding encoding)
        {
            if (encoding == null) encoding = DefaultEncoding;

            file.Directory.EnsureExists();

            if (encoding is UTF8Encoding) encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

            return Task.Run(() => File.WriteAllText(file.FullName, content, encoding));
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
        public static Task AppendAllTextAsync(this FileInfo file, string content, Encoding encoding)
        {
            if (encoding == null) encoding = DefaultEncoding;

            file.Directory.EnsureExists();

            return Task.Run(() => File.AppendAllText(file.FullName, content, encoding));
        }

        /// <summary>
        /// Compresses this data into Gzip.
        /// </summary>
        public static async Task<byte[]> GZipAsync(this byte[] data)
        {
            using (var outFile = new MemoryStream())
            {
                using (var inFile = new MemoryStream(data))
                using (var Compress = new GZipStream(outFile, CompressionMode.Compress))
                    await inFile.CopyToAsync(Compress);

                return outFile.ToArray();
            }
        }

        /// <summary>
        /// Compresses this string into Gzip. By default it will use UTF8 encoding.
        /// </summary>
        public static Task<byte[]> GZipAsync(this string data) => GZipAsync(data, Encoding.UTF8);

        /// <summary>
        /// Compresses this string into Gzip.
        /// </summary>
        public static Task<byte[]> GZipAsync(this string data, Encoding encoding)
            => encoding.GetBytes(data).GZipAsync();
    }
}