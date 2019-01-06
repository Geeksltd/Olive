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
        public static void CopyTo(this FileInfo @this, FileInfo destinationPath, bool overwrite = true)
        {
            if (!overwrite && destinationPath.Exists()) return;

            File.Copy(@this.ExistsOrThrow().FullName, destinationPath.FullName, overwrite);
        }

        /// <summary>
        /// Gets the entire content of this file.
        /// </summary>
        public static byte[] ReadAllBytes(this FileInfo file)
            => TryHard(file, () => File.ReadAllBytes(file.FullName), "The system cannot read the file: {0}");

        /// <summary>
        /// Gets the entire content of this file.
        /// </summary>
        public static string ReadAllText(this FileInfo @this) => ReadAllText(@this, DefaultEncoding);

        /// <summary>
        /// Gets the entire content of this file.
        /// </summary>
        public static string ReadAllText(this FileInfo @this, Encoding encoding)
        {
            Func<string> readFile = () =>
            {
                using (var stream = new FileStream(@this.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var reader = new StreamReader(stream, encoding))
                        return reader.ReadToEnd();
                }
            };

            return TryHard(@this, readFile, "The system cannot read the file: {0}");
        }

        public static void DeleteIfExists(this FileInfo @this)
        {
            if (@this != null && @this.Exists()) @this.Delete();
        }

        /// <summary>
        /// Will try to delete a specified file if it exists.
        /// </summary>
        /// <param name="harshly">If set to true, then it will try multiple times, in case the file is temporarily locked.</param>
        public static void Delete(this FileInfo @this, bool harshly)
        {
            if (!@this.Exists()) return;

            if (!harshly)
            {
                Task.Factory.StartNew(@this.Delete);
                return;
            }

            DoTryHard(@this, () => @this.Delete(),
               "The system cannot delete the file, even after several attempts. Path: {0}");
        }

        /// <summary>
        /// Saves the specified content on this file.
        /// </summary>
        public static void WriteAllBytes(this FileInfo @this, byte[] content)
        {
            if (!@this.Directory.Exists())
                @this.Directory.Create();

            DoTryHard(@this, () => File.WriteAllBytes(@this.FullName, content),
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
        public static void WriteAllText(this FileInfo @this, string content, Encoding encoding)
        {
            if (encoding == null) encoding = DefaultEncoding;

            @this.Directory.EnsureExists();

            if (encoding is UTF8Encoding) encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

            File.WriteAllText(@this.FullName, content, encoding);
        }

        /// <summary>
        /// Saves the specified content to the end of this file.
        /// </summary>
        public static void AppendAllText(this FileInfo @this, string content)
            => AppendAllText(@this, content, DefaultEncoding);

        /// <summary>
        /// Saves the specified content to the end of this file.
        /// </summary>
        public static async Task AppendAllTextAsync(this FileInfo @this, string content)
        {
            using (await @this.GetSyncLock().Lock())
            {
                @this.Directory.EnsureExists();
                using (var streamWriter = File.AppendText(@this.FullName))
                    await streamWriter.WriteAsync(content);
            }
        }

        /// <summary>
        /// Saves the specified content to the end of this file.
        /// </summary>
        public static void AppendLine(this FileInfo @this, string content = null)
            => AppendAllText(@this, content + Environment.NewLine, DefaultEncoding);

        /// <summary>
        /// Saves the specified content to the end of this file.
        /// </summary>
        public static void AppendAllText(this FileInfo @this, string content, Encoding encoding)
        {
            if (encoding == null) encoding = DefaultEncoding;

            @this.Directory.EnsureExists();

            File.AppendAllText(@this.FullName, content, encoding);
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
        public static byte[] UnGZip(this byte[] @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            using (var zippedStream = @this.AsStream())
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
        public static byte[] GZip(this string @this, Encoding encoding)
            => encoding.GetBytes(@this).GZip();
    }
}