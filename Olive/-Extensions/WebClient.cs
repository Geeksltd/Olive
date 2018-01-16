using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveExtensions
    {
        /// <summary>
        /// First three bytes of GZip compressed Data
        /// </summary>
        readonly static byte[] GZipStarter = new byte[] { 31, 139, 8 };

        public static async Task<byte[]> DownloadData(this WebClient client, string address, bool handleGzip)
        {
            if (!handleGzip)
                return await client.DownloadDataTaskAsync(address);

            var result = await client.DownloadDataTaskAsync(address);
            if (result != null && result.Length > 3 && result[0] == GZipStarter[0] && result[1] == GZipStarter[1] && result[2] == GZipStarter[2])
            {
                // GZIP:
                using (var stream = new System.IO.Compression.GZipStream(new MemoryStream(result), System.IO.Compression.CompressionMode.Decompress))
                {
                    var buffer = new byte[4096];
                    using (var memory = new MemoryStream())
                    {
                        while (true)
                        {
                            var count = await stream.ReadAsync(buffer, 0, buffer.Length);
                            if (count > 0) await memory.WriteAsync(buffer, 0, count);
                            else break;
                        }

                        return memory.ToArray();
                    }
                }
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// Posts the specified data to a url and returns the response as string.
        /// All properties of the postData object will be sent as individual FORM parameters to the destination.
        /// </summary>
        /// <param name="postData">An anonymous object containing post data.</param>
        public static async Task<string> Post(this WebClient webClient, string url, object postData)
        {
            if (postData == null)
                throw new ArgumentNullException(nameof(postData));

            var data = new Dictionary<string, string>();
            data.AddFromProperties(postData);

            return await Post(webClient, url, data);
        }

        /// <summary>
        /// Posts the specified data to a url and returns the response as string.
        /// All items in the postData object will be sent as individual FORM parameters to the destination.
        /// </summary>
        public static async Task<string> Post(this WebClient webClient, string url, Dictionary<string, string> postData) =>
            await Post(webClient, url, postData, Encoding.UTF8);

        /// <summary>
        /// Posts the specified data to a url and returns the response as string.
        /// </summary>
        public static async Task<string> Post(this WebClient webClient, string url, Dictionary<string, string> postData, Encoding responseEncoding)
        {
            if (responseEncoding == null)
                throw new ArgumentNullException(nameof(responseEncoding));

            if (postData == null)
                throw new ArgumentNullException(nameof(postData));

            if (url.IsEmpty())
                throw new ArgumentNullException(nameof(url));

            var responseBytes = await webClient.UploadValuesTaskAsync(url, postData.ToNameValueCollection());

            try
            {
                return responseEncoding.GetString(responseBytes);
            }
            catch (WebException ex)
            {
                throw new Exception(await ex.GetResponseBody());
            }
        }
    }
}