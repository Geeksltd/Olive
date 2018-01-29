using System.Net;
using Olive.Web;

namespace Olive
{
    public static class ApiClientExtensions
    {
        /// <summary>
        /// Passes the identity of the current http user on to the api.
        /// </summary>
        public static ApiClient AsHttpUser(this ApiClient client)
        {
            var cookieName = ".myAuth"; // TODO: Get it from the cookie settings.
            client.Authenticate(new Cookie(cookieName, Context.Http.Request.Cookies[cookieName]));
            return client;
        }
    }
}
