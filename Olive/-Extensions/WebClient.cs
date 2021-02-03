using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveExtensions
    {
        /// <summary>
        /// First three bytes of GZip compressed Data
        /// </summary>
        static readonly byte[] GZipStarter = new byte[] { 31, 139, 8 };

        public static async Task<byte[]> DownloadData(this HttpClient client, string address, bool handleGzip)
        {
            if (!handleGzip)
                return await client.GetByteArrayAsync(address);

            var result = await client.GetByteArrayAsync(address);
            if (result != null && result.Length > 3 && result[0] == GZipStarter[0] && result[1] == GZipStarter[1] && result[2] == GZipStarter[2])
            {
                // GZIP:
                using var stream = new System.IO.Compression.GZipStream(new MemoryStream(result), System.IO.Compression.CompressionMode.Decompress);
                var buffer = new byte[4096];
                using var memory = new MemoryStream();
                while (true)
                {
                    var count = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (count > 0) await memory.WriteAsync(buffer, 0, count);
                    else break;
                }

                return memory.ToArray();
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
        /// <param name="url">Post url address</param>
        /// <param name="postData">An anonymous object containing post data.</param>
        /// <param name="httpClient">HttpClient instance</param>
        public static async Task<string> Post(this HttpClient httpClient, string url, object postData)
        {
            if (postData == null)
                throw new ArgumentNullException(nameof(postData));

            var data = new Dictionary<string, string>();
            data.AddFromProperties(postData);

            return await Post(httpClient, url, data);
        }

        /// <summary>
        /// Posts the specified data to a url and returns the response as string.
        /// All items in the postData object will be sent as individual FORM parameters to the destination.
        /// </summary>
        public static async Task<string> Post(this HttpClient httpClient, string url, Dictionary<string, string> postData) =>
            await Post(httpClient, url, postData, Encoding.UTF8);

        /// <summary>
        /// Posts the specified data to a url and returns the response as string.
        /// </summary>
        public static async Task<string> Post(this HttpClient httpClient, string url, Dictionary<string, string> postData, Encoding responseEncoding)
        {
            if (responseEncoding == null)
                throw new ArgumentNullException(nameof(responseEncoding));

            if (postData == null)
                throw new ArgumentNullException(nameof(postData));

            if (url.IsEmpty())
                throw new ArgumentNullException(nameof(url));

            var responseBytes = await httpClient.PostAsync(url, postData.AsHttpContent());

            try
            {
                return responseEncoding.GetString(await responseBytes.Content.ReadAsByteArrayAsync());
            }
            catch (WebException ex)
            {
                throw new Exception(await ex.GetResponseBody());
            }
        }
    }
}