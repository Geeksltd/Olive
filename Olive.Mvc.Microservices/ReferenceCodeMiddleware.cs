using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Olive.Mvc.Microservices
{
    /// <summary>
    /// Gives every request a short reference code, which is attached to the context of any log
    /// entry written during that request, and returned to the caller as a response header.
    /// When something fails, the user can be shown the code and support can search the logs for it.
    /// </summary>
    public class ReferenceCodeMiddleware
    {
        public const string HeaderName = "X-Reference-Code";

        // No 0/O/1/I: users read these codes out to support.
        const string Alphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";
        const int Length = 8;

        readonly RequestDelegate Next;

        public ReferenceCodeMiddleware(RequestDelegate next) => Next = next;

        public Task Invoke(HttpContext httpContext)
        {
            var code = Generate();

            httpContext.Items[Log.ReferenceCodeKey] = code;

            // Setting the header here rather than after Next() so that it survives the exception
            // path (where the response is written by the exception handler) and so that we never
            // attempt to add a header to a response which has already started.
            httpContext.Response.OnStarting(o =>
            {
                var response = ((HttpContext)o).Response;
                if (!response.Headers.ContainsKey(HeaderName))
                    response.Headers[HeaderName] = code;

                return Task.CompletedTask;
            }, httpContext);

            return Next(httpContext);
        }

        static string Generate()
        {
            var bytes = new byte[Length];
            using (var random = RandomNumberGenerator.Create())
                random.GetBytes(bytes);

            var result = new StringBuilder("REF-", 4 + Length);

            foreach (var item in bytes)
                result.Append(Alphabet[item % Alphabet.Length]);

            return result.ToString();
        }
    }
}
