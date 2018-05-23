﻿using System.Net;
using System.Linq;

namespace Olive
{
    public static partial class OliveWebExtensions
    {
        /// <summary>
        /// Passes the identity of the current http user on to the api.
        /// </summary>
        public static ApiClient AsHttpUser(this ApiClient client)
        {
            var cookieName = ".myAuth"; // TODO: Get it from the cookie settings.

            var request = Context.Current.Request();
            var domain = client.Url.AsUri().Host;

            var cookies = request.Cookies
                .Where(x => x.Key == cookieName || x.Key.StartsWith(cookieName + "C"))
                .Select(x => new Cookie(x.Key, x.Value) { Domain = domain })
                .ToArray();

            client.Authenticate(cookies);
            return client;
        }
    }
}
