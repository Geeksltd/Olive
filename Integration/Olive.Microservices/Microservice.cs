using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Olive
{
    /// <summary>
    /// Provides helper services for implementing microservices using Olive.
    /// </summary>
    public class Microservice
    {
        static bool IsAuthenticated;
        internal static Cookie[] ServiceIdentityCookies;

        /// <summary>
        /// Gets the name of the current microservice from the config value of Microservice:Name.
        /// </summary>
        public static string Name => Config.GetOrThrow("Microservice:Name");

        /// <summary>
        /// Gets the value of the config key Microservice:Root.Domain.
        /// </summary>
        public static string RootDomain => Config.GetOrThrow("Microservice:Root.Domain");

        /// <summary>
        /// Gets 'http'or 'https' baesd on the curretn config value of Microservice:Http.Protocol.
        /// </summary>
        public static string HttpProtocol => Config.GetOrThrow("Microservice:Http.Protocol");

        /// <summary>
        /// For example for the specified service name of 'auth' 
        /// it will return the current full url such as https://auth.my-services.com.
        /// </summary>
        public static string Url(string serviceName, string relativeUrl = null)
        {
            return HttpProtocol + "://" + serviceName + "." + RootDomain + relativeUrl.EnsureStartsWith("/");
        }

        /// <summary>
        /// Authenticates me based on the auth service url of {auth.service}/api/login/service/{Name}/{secret} 
        /// and returns the authentication cookie value.
        /// Note: Secret is the config value of Microservice:Secret. It should be registered in the Auth service also.
        /// </summary>
        public static async Task Authenticate()
        {
            if (IsAuthenticated) return;

            var key = Config.GetOrThrow("Microservice:Secret");

            await Authenticate("auth", $"api/login/service/{Name}/{key}");

            IsAuthenticated = true;
        }

        /// <summary>
        /// Authenticates me by sending a Http Get request to the specified auth service url and returns the authentication cookie value from the response cookies.        
        /// </summary>
        public static async Task Authenticate(string authServiceName, string relativeAuthUrl)
        {
            var url = Url(authServiceName, relativeAuthUrl);

            var authCookieName = ".myAuth"; // TODO: Get it from the cookie settings.

            using (var handler = new HttpClientHandler { CookieContainer = new CookieContainer() })
            using (var client = new HttpClient(handler))
            {
                var response = await client.GetAsync(url);

                var responseCookies = handler.CookieContainer
                    .GetCookies(Url(authServiceName).AsUri())
                    .GetCookieOrChunks(authCookieName);

                if (responseCookies.None())
                    throw new Exception("Service authentication failed.");

                ServiceIdentityCookies = responseCookies;
            }
        }

        /// <summary>
        /// Creates an Api client to invoke an api of a specified service.
        /// </summary>
        public static ApiClient Api(string serviceName, string relativeApiUrl)
            => new ApiClient(Url(serviceName, relativeApiUrl));
    }
}