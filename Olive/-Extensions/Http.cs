using System.IO;
using System.Net;
using System.Linq;
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
            using (var response = request.GetResponse())
                return await response.GetString();
        }

        /// <summary>
        /// Gets the response data as string.
        /// </summary>
        public static async Task<string> GetString(this WebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                    return await reader.ReadToEndAsync();
            }
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
    }
}