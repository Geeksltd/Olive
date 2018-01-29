using System;
using System.Net;

namespace Olive
{
    partial class ApiClient
    {
        readonly CookieContainer RequestCookies = new CookieContainer();
        static Cookie[] ServiceIdentityCookies;


        /// <summary>Passes the identity of this service.</summary>
        public ApiClient AsServiceUser()
        {
            if (ServiceIdentityCookies.None())
                throw new Exception("This service is not authenticated yet. For ApiClient.AsService() to work " +
                    "you should first authenticate the service in Startup.cs and receive a valid cookie.");

            Authenticate(ServiceIdentityCookies);
            return this;
        }

        public ApiClient Authenticate(params Cookie[] identityCookies)
        {
            foreach (var item in identityCookies) RequestCookies.Add(item);
            return this;
        }

        public static void SignInAsService(Cookie[] serviceIdentityCookies)
            => ServiceIdentityCookies = serviceIdentityCookies;
    }
}