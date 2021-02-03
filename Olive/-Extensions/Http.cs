using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveExtensions
    {
        /// <summary>
        /// Gets the response data as string.
        /// </summary>
        public static async Task<string> GetResponseString(this HttpWebRequest request)
        {
            using var response = request.GetResponse();
            return await response.GetString();
        }

        /// <summary>
        /// Determines whether this is in the 400 range.
        /// </summary>
        public static bool ContainsUserMessage(this HttpStatusCode @this)
        {
            var code = (int)@this;

            return code >= (int)HttpStatusCode.BadRequest &&
                            code < (int)HttpStatusCode.InternalServerError;
        }

        /// <summary>
        /// Determines whether this is in the 400 or 500 ranges.
        /// </summary>
        public static bool IsError(this HttpStatusCode @this)
        {
            var code = (int)@this;
            return code >= (int)HttpStatusCode.BadRequest;
        }

        /// <summary>
        /// Gets the response data as string.
        /// </summary>
        public static async Task<string> GetString(this WebResponse response)
        {
            using var stream = response.GetResponseStream();
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// Gets the cookie or cookie chunks for the specified cookie name.
        /// </summary>
        public static Cookie[] GetCookieOrChunks(this CookieCollection cookies, string name)
        {
            return cookies.Cast<Cookie>()
                   .Where(x => x.Name == name ||
                   (x.Name.StartsWith(name + "C") && x.Name.TrimStart(name + "C").Is<int>()))
                   .ToArray();
        }

        /// <summary>
        /// Sends a Http Post message.
        /// </summary>
        /// <param name="anonymouseObject">Each property in this anonymous object will be sent as a form field.</param>        
        public static Task<HttpResponseMessage> PostFormAsync(this HttpClient @this, string url, object anonymouseObject)
        {
            var settings =
                anonymouseObject.GetType().GetProperties()
                .ToDictionary(x => x.Name, x => x.GetValue(anonymouseObject)?.ToStringOrEmpty());

            return @this.PostFormAsync(url, settings);
        }

        /// <summary>
        /// Sends a Http Post message.
        /// </summary>
        public static Task<HttpResponseMessage> PostFormAsync(this HttpClient @this,
            string url, IEnumerable<KeyValuePair<string, string>> formData)
        {
            return @this.PostAsync(url, new FormUrlEncodedContent(formData));
        }
    }
}