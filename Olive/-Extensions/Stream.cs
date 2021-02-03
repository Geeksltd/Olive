using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveExtensions
    {
        /// <summary>
        /// Will set the Position to zero, and then copy all bytes to a memory stream's buffer.
        /// </summary>
        public static byte[] ReadAllBytes(this Stream stream)
        {
            using var memoryStream = new MemoryStream();
            try { if (stream.CanSeek) stream.Position = 0; }
            catch (NotSupportedException) { /*Not needed*/ }
            try { stream.CopyTo(memoryStream); }
            catch (System.IO.InvalidDataException) { /*Not needed*/ }
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Will set the Position to zero, and then copy all bytes to a memory stream's buffer.
        /// </summary>
        public static async Task<byte[]> ReadAllBytesAsync(this Stream stream)
        {
            using var memoryStream = new MemoryStream();
            if (stream.CanSeek) stream.Position = 0;
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Reads all text in this stream as UTF8.
        /// </summary>
        /// <param name="encoding">If not specified (or NULL specified) then UTF8 will be used.</param>
        public static async Task<string> ReadAllText(this Stream @this, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;

            // Pipes the stream to a higher level stream reader with the required encoding format.
            using var readStream = new StreamReader(@this, encoding);
            var result = "";

            var read = new char[256];
            // Reads 256 characters at a time.
            var count = await readStream.ReadAsync(read, 0, read.Length);

            while (count > 0)
            {
                // Dumps the 256 characters on a string and displays the string to the console.
                result += new string(read, 0, count);
                count = await readStream.ReadAsync(read, 0, read.Length);
            }

            return result;
        }

        public static byte[] ReadAllBytes(this BinaryReader reader)
        {
            const int BUFFER_SIZE = 4096;
            using var ms = new MemoryStream();
            var buffer = new byte[BUFFER_SIZE];
            int count;
            while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                ms.Write(buffer, 0, count);
            return ms.ToArray();
        }

        public static MemoryStream AsStream(this byte[] data) => new MemoryStream(data);
    }
}
