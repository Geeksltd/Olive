using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Olive.Entities;

namespace Olive.Web
{
    public static partial class OliveExtensions
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
        /// Gets the response data as string.
        /// </summary>
        public static async Task<string> GetResponseString(this HttpWebRequest request)
        {
            using (var response = request.GetResponse())
                return await response.GetString();
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

        public static string ToAuditDataHtml(this IApplicationEvent applicationEvent, bool excludeIds = false)
        {
            if (applicationEvent.Event == "Insert" && applicationEvent.Data.OrEmpty().StartsWith("<Data>"))
            {
                // return applicationEvent.Data.To<XElement>().Elements().Select(p => $"<div class='prop'><span class='key'>{p.Name}</span>: <span class='val'>{p.Value.HtmlEncode()}</span></div>").ToLinesString();

                var insertData = applicationEvent.Data.To<XElement>().Elements().ToArray();

                if (excludeIds)
                    insertData = insertData.Except(x => x.Name.LocalName.EndsWith("Id") && insertData.Select(p => p.Name.LocalName).Contains(x.Name.LocalName.TrimEnd("Id")))
                         .Except(x => x.Name.LocalName.EndsWith("Ids") && insertData.Select(p => p.Name.LocalName).Contains(x.Name.LocalName.TrimEnd("Ids"))).ToArray();

                return insertData.Select(p => $"<div class='prop'><span class='key'>{p.Name.LocalName.ToLiteralFromPascalCase()}</span>: <span class='val'>{p.Value.HtmlEncode()}</span></div>").ToLinesString();
            }

            if (applicationEvent.Event == "Update" && applicationEvent.Data.OrEmpty().StartsWith("<DataChange>"))
            {
                var data = applicationEvent.Data.To<XElement>();
                var old = data.Element("old");
                var newData = data.Element("new");
                var propertyNames = old.Elements().Select(x => x.Name.LocalName)
                    .Concat(newData.Elements().Select(x => x.Name.LocalName)).Distinct().ToArray();

                if (excludeIds)
                    propertyNames = propertyNames.Except(p => p.EndsWith("Id") && propertyNames.Contains(p.TrimEnd("Id")))
                         .Except(p => p.EndsWith("Ids") && propertyNames.Contains(p.TrimEnd("Ids"))).ToArray();

                return propertyNames.Select(p => $"<div class='prop'>Changed <span class='key'>{p.ToLiteralFromPascalCase()}</span> from <span class='old'>\"{ old.GetValue<string>(p).HtmlEncode() }\"</span> to <span class='new'>\"{ newData.GetValue<string>(p).HtmlEncode() }\"</span></div>").ToLinesString();
            }

            if (applicationEvent.Event == "Delete" && applicationEvent.Data.OrEmpty().StartsWith("<DataChange>"))
            {
                var data = applicationEvent.Data.To<XElement>();
                var old = data.Element("old");

                var propertyNames = old.Elements().Select(x => x.Name.LocalName).ToArray();

                if (excludeIds)
                    propertyNames = propertyNames.Except(p => p.EndsWith("Id") && propertyNames.Contains(p.TrimEnd("Id")))
                         .Except(p => p.EndsWith("Ids") && propertyNames.Contains(p.TrimEnd("Ids"))).ToArray();

                return propertyNames.Select(p => $"<div class='prop'><span class='key'>{p.ToLiteralFromPascalCase()}</span> was <span class='old'>\"{old.GetValue<string>(p).HtmlEncode() }\"</span></div>").ToLinesString();
            }

            return applicationEvent.Data.OrEmpty().HtmlEncode();
        }

        public static ClaimsPrincipal CreateClaimsPrincipal(this IIdentity user, IEnumerable<string> roles, string authType)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, (user is IEntity ent) ? ent.GetId().ToString() : user.Name)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));

            return new ClaimsPrincipal(new ClaimsIdentity(claims, authType));
        }
    }
}
