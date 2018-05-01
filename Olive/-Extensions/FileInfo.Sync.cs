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
        public static void CopyTo(this FileInfo file, FileInfo destinationPath, bool overwrite = true)
        {
            if (!overwrite && destinationPath.Exists()) return;
            if (!file.Exists()) throw new Exception("File does not exist: " + file.FullName);

            File.Copy(file.FullName, destinationPath.FullName, overwrite);
        }

        /// <summary>
        /// Gets the entire content of this file.
        /// </summary>
        public static byte[] ReadAllBytes(this FileInfo file)
            => TryHard(file, () => File.ReadAllBytes(file.FullName), "The system cannot read the file: {0}");

        /// <summary>
        /// Gets the entire content of this file.
        /// </summary>
        public static string ReadAllText(this FileInfo file) => ReadAllText(file, DefaultEncoding);

        /// <summary>
        /// Gets the entire content of this file.
        /// </summary>
        public static string ReadAllText(this FileInfo file, Encoding encoding)
        {
            Func<string> readFile = () =>
            {
                using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var reader = new StreamReader(stream, encoding))
                        return reader.ReadToEnd();
                }
            };

            return TryHard(file, readFile, "The system cannot read the file: {0}");
        }

        public static void DeleteIfExists(this FileInfo file)
        {
            if (file != null && file.Exists()) file.Delete();
        }

        /// <summary>
        /// Will try to delete a specified file if it exists.
        /// </summary>
        /// <param name="harshly">If set to true, then it will try multiple times, in case the file is temporarily locked.</param>
        public static void Delete(this FileInfo file, bool harshly)
        {
            if (!file.Exists()) return;

            if (!harshly)
            {
                Task.Factory.StartNew(file.Delete);
                return;
            }

            DoTryHard(file, () => file.Delete(harshly),
               "The system cannot delete the file, even after several attempts. Path: {0}");
        }

        /// <summary>
        /// Saves the specified content on this file.
        /// </summary>
        public static void WriteAllBytes(this FileInfo file, byte[] content)
        {
            if (!file.Directory.Exists())
                file.Directory.Create();

            DoTryHard(file, () => File.WriteAllBytes(file.FullName, content),
               "The system cannot write the specified content on the file: {0}");
        }

        /// <summary>
        /// Saves the specified content on this file using the Western European Windows Encoding 1252.
        /// </summary>
        public static void WriteAllText(this FileInfo file, string content)
            => WriteAllText(file, content, DefaultEncoding);

        /// <summary>
        /// Saves the specified content on this file. 
        /// Note: For backward compatibility, for UTF-8 encoding, it will always add the BOM signature.
        /// </summary>
        public static void WriteAllText(this FileInfo file, string content, Encoding encoding)
        {
            if (encoding == null) encoding = DefaultEncoding;

            file.Directory.EnsureExists();

            if (encoding is UTF8Encoding) encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

            File.WriteAllText(file.FullName, content, encoding);
        }

        /// <summary>
        /// Saves the specified content to the end of this file.
        /// </summary>
        public static void AppendAllText(this FileInfo file, string content)
            => AppendAllText(file, content, DefaultEncoding);

        /// <summary>
        /// Saves the specified content to the end of this file.
        /// </summary>
        public static async Task AppendAllTextAsync(this FileInfo file, string content)
        {
            using (await file.GetSyncLock().Lock())
            {
                file.Directory.EnsureExists();
                using (var streamWriter = File.AppendText(file.FullName))
                    await streamWriter.WriteAsync(content);
            }
        }

        /// <summary>
        /// Saves the specified content to the end of this file.
        /// </summary>
        public static void AppendLine(this FileInfo file, string content = null)
            => AppendAllText(file, content + Environment.NewLine, DefaultEncoding);

        /// <summary>
        /// Saves the specified content to the end of this file.
        /// </summary>
        public static void AppendAllText(this FileInfo file, string content, Encoding encoding)
        {
            if (encoding == null) encoding = DefaultEncoding;

            file.Directory.EnsureExists();

            File.AppendAllText(file.FullName, content, encoding);
        }

        /// <summary>
        /// Compresses this data into Gzip.
        /// </summary>
        public static byte[] GZip(this byte[] data)
        {
            using (var outFile = new MemoryStream())
            {
                using (var inFile = new MemoryStream(data))
                using (var compress = new GZipStream(outFile, CompressionMode.Compress))
                    inFile.CopyTo(compress);

                return outFile.ToArray();
            }
        }

        /// <summary>
        /// Decompresses this gzipped data.
        /// </summary>
        public static byte[] UnGZip(this byte[] zippedData)
        {
            if (zippedData == null)
                throw new ArgumentNullException(nameof(zippedData));

            using (var zippedStream = zippedData.AsStream())
            using (var decompress = new GZipStream(zippedStream, CompressionMode.Decompress))
                return decompress.ReadAllBytes();
        }

        /// <summary>
        /// Compresses this string into Gzip. By default it will use UTF8 encoding.
        /// </summary>
        public static byte[] GZip(this string data) => GZip(data, Encoding.UTF8);

        /// <summary>
        /// Compresses this string into Gzip.
        /// </summary>
        public static byte[] GZip(this string data, Encoding encoding)
            => encoding.GetBytes(data).GZip();
    }
}