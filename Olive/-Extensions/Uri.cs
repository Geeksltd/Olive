using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Olive
{
    partial class OliveExtensions
    {
        /// <summary>
        /// Downloads the text in this URL.
        /// </summary>
        public static async Task<string> Download(this Uri url, string cookieValue = null, double timeOutSeconds = 60)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Timeout = (int)(timeOutSeconds * 1000);

            if (cookieValue.HasValue())
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.SetCookies(url, cookieValue.OrEmpty());
            }

            using (var response = await request.GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                    return await stream.ReadAllText();
            }
        }

        /// <summary>
        /// Downloads the data in this URL.
        /// </summary>
        public static async Task<byte[]> DownloadData(this Uri url, string cookieValue = null, double timeOutSeconds = 60)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(url);

            request.Timeout = (int)(timeOutSeconds * 1000);

            if (cookieValue.HasValue())
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.SetCookies(url, cookieValue.OrEmpty());
            }

            using (var response = await request.GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                    return await stream.ReadAllBytesAsync();
            }
        }

        /// <summary>
        /// Posts the specified object as JSON data to this URL.
        /// </summary>
        public static async Task<string> PostJson(this Uri url, object data)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = WebRequestMethods.Http.Post;
            req.ContentType = "application/json";

            using (var stream = new StreamWriter(await req.GetRequestStreamAsync()))
                await stream.WriteAsync(JsonConvert.SerializeObject(data));

            return await req.GetResponseString();
        }

        /// <summary>
        /// Posts the specified data to this url and returns the response as string.
        /// All items in the postData object will be sent as individual FORM parameters to the destination.
        /// </summary>
        public static async Task<string> Post(this Uri url, object data, Action<WebClient> customiseClient = null)
        {
            using (var client = new WebClient())
            {
                customiseClient?.Invoke(client);

                return await client.Post(url.ToString(), data);
            }
        }

        /// <summary>
        /// Posts the specified data to this url and returns the response as string.
        /// All items in the postData object will be sent as individual FORM parameters to the destination.
        /// </summary>
        public static async Task<string> Post(this Uri url, Dictionary<string, string> postData, Action<WebClient> customiseClient = null)
        {
            using (var client = new WebClient())
            {
                customiseClient?.Invoke(client);
                return await client.Post(url.ToString(), postData);
            }
        }
    }
}