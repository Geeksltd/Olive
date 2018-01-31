using System;
using System.Net;

namespace Olive
{
    partial class ApiClient
    {
        readonly CookieContainer RequestCookies = new CookieContainer();

        public ApiClient Authenticate(params Cookie[] identityCookies)
        {
            foreach (var item in identityCookies) RequestCookies.Add(item);
            return this;
        }
    }
}