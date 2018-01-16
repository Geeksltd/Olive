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
            using (var memoryStream = new MemoryStream())
            {
                stream.Position = 0;
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Will set the Position to zero, and then copy all bytes to a memory stream's buffer.
        /// </summary>
        public static async Task<byte[]> ReadAllBytesAsync(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.Position = 0;
                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Reads all text in this stream as UTF8.
        /// </summary>
        /// <param name="encoding">If not specified (or NULL specified) then UTF8 will be used.</param>
        public static async Task<string> ReadAllText(this Stream response, Encoding encoding = null)
        {
            var result = "";

            // Pipes the stream to a higher level stream reader with the required encoding format.
            using (var readStream = new StreamReader(response, encoding))
            {
                var read = new char[256];
                // Reads 256 characters at a time.
                var count = await readStream.ReadAsync(read, 0, read.Length);

                while (count > 0)
                {
                    // Dumps the 256 characters on a string and displays the string to the console.
                    result += new string(read, 0, count);
                    count = await readStream.ReadAsync(read, 0, read.Length);
                }
            }

            return result;
        }
    }
}