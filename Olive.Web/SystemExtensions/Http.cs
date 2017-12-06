namespace Olive.Web
{
    partial class OliveExtensions
    {
        /// <summary>
        /// Determines whether this is an Ajax call.
        /// </summary>
        public static bool IsAjaxRequest(this HttpRequest request) => request.IsAjaxCall();

        /// <summary>
        /// Determines whether this is an Ajax call.
        /// </summary>
        public static bool IsAjaxCall(this HttpRequest request) => request.Headers["X-Requested-With"] == "XMLHttpRequest";

        /// <summary>
        /// Dispatches a binary data block back to the client.
        /// </summary>
        public static async Task Dispatch(this HttpResponse response, byte[] responseData, string fileName, string contentType = "Application/octet-stream")
        {
            if (responseData == null)
                throw new ArgumentNullException(nameof(responseData));

            if (fileName.IsEmpty())
                throw new ArgumentNullException(nameof(fileName));

            response.Clear();
            response.ContentType = contentType;

            response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName.Remove("\"").Replace(",", "-")}\"");

            await response.Body.WriteAsync(responseData, 0, responseData.Length);
            await response.Body.FlushAsync();
        }

        /// <summary>
        /// Dispatches a string back to the client as a file.
        /// </summary>
        public static void Dispatch(this HttpResponse response, string responseText, string fileName, string contentType = "Application/octet-stream", System.Text.Encoding encoding = null)
        {
            response.Clear();

            response.Headers.Add("Cache-Control", "no-store");
            response.Headers.Add("Pragma", "no-cache");

            if (fileName.HasValue())
                response.Headers.Add("Content-Disposition", $"attachment;filename={fileName.Replace(" ", "_")}");

            response.ContentType = contentType;

            if (encoding != null)
                response.WriteAsync(responseText, encoding).RunSynchronously();
            else
                response.WriteAsync(responseText).RunSynchronously();
        }

        /// <summary>
        /// Dispatches a file back to the client.
        /// </summary>
        /// <param name="fileName">If set to null, the same file name of the file will be used.</param>
        public static async Task Dispatch(this HttpResponse response, FileInfo responseFile, string fileName = null, string contentType = "Application/octet-stream")
        {
            if (responseFile == null)
                throw new ArgumentNullException(nameof(responseFile));

            if (fileName.IsEmpty())
                fileName = responseFile.Name;

            await Dispatch(response, await responseFile.ReadAllBytes(), fileName, contentType);
        }

        /// <summary>
        /// Dispatches a file back to the client.
        /// </summary>
        public static async Task Dispatch(this HttpResponse response, Blob blob, string contentType = "Application/octet-stream")
        {
            await Dispatch(response, await File.ReadAllBytesAsync(blob.LocalPath), blob.FileName, contentType);
        }

        /// <summary>
        /// Determines if this is a GET http request.
        /// </summary>
        public static bool IsGet(this HttpRequest request) => request.Method == System.Net.WebRequestMethods.Http.Get;

        /// <summary>
        /// Determines if this is a POST http request.
        /// </summary>
        public static bool IsPost(this HttpRequest request) => request.Method == System.Net.WebRequestMethods.Http.Post;

        /// <summary>
        /// Gets the currently specified return URL.
        /// </summary>
        public static string GetReturnUrl(this HttpRequest request)
        {
            var result = request.Param("ReturnUrl");

            if (result.IsEmpty()) return string.Empty;

            if (result.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                result.ToCharArray().ContainsAny('\'', '\"', '>', '<') ||
                result.ContainsAny(new[] { "//", ":" }, caseSensitive: false))
                throw new Exception("Invalid ReturnUrl.");

            return result;
        }

        /// <summary>
        /// Writes the specified message in the response and then ends the response.
        /// </summary>
        public static void EndWith(this HttpResponse response, string message, string mimeType = "text/html")
        {
            response.ContentType = mimeType;
            response.WriteAsync(message).RunSynchronously();
        }

        /// <summary>
        /// Reads the full content of a posted text file.
        /// </summary>
        public static async Task<string> ReadAllText(this IFormFile file)
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
                return await reader.ReadToEndAsync();
        }

        public static bool IsLocal(this HttpRequest req)
        {
            var connection = req.HttpContext.Connection;
            if (connection.RemoteIpAddress != null)
            {
                if (connection.LocalIpAddress != null)
                    return connection.RemoteIpAddress.Equals(connection.LocalIpAddress);
                else
                    return IPAddress.IsLoopback(connection.RemoteIpAddress);
            }

            // for in memory TestServer or when dealing with default connection info
            if (connection.RemoteIpAddress == null && connection.LocalIpAddress == null)
                return true;

            return false;
        }
    }
}