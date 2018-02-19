using System;
using System.Net;

namespace Olive
{
    partial class ApiClient
    {
        CookieContainer RequestCookies = new CookieContainer();

        public ApiClient Authenticate(params Cookie[] identityCookies)
        {
            RequestCookies = new CookieContainer();
            foreach (var item in identityCookies) RequestCookies.Add(item);
            return this;
        }
    }
}