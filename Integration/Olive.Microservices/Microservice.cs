using System.Net;

namespace Olive
{
    /// <summary>
    /// Provides helper services for implementing microservices using Olive.
    /// </summary>
    public class Microservice
    {
        public string Name { get; private set; }
        string BaseUrl { get; set; }
        string AccessKey { get; set; }

        Microservice(string name) => Name = name;

        public static Microservice Of(string serviceName)
        {
            return new Microservice(serviceName)
            {
                BaseUrl = Config.GetOrThrow("Microservice:" + serviceName + ":Url"),
                AccessKey = Config.Get("Microservice:" + serviceName + ":AccessKey")
            };
        }

        public static Microservice Me
        {
            get
            {
                return new Microservice(Config.GetOrThrow("Microservice:Me:Name"))
                {
                    BaseUrl = Config.Get("Microservice:Me:Url").Or("Config value not specified for 'Microservice:Me:Url'")
                };
            }
        }

        /// <summary>
        /// Returns the full url to a specified resource in this microservice by
        /// concatinating the base url of this service with the specified relative url.
        /// </summary>
        public string Url(string relativeUrl = null)
        {
            return BaseUrl + relativeUrl.OrEmpty().TrimStart("/").EnsureStartsWith("/");
        }

        /// <summary>
        /// Creates an Api client for this service.
        /// It will automatically add an authentication cookie based on the service key.
        /// </summary>
        public ApiClient Api(string relativeApiUrl)
        {
            var authCookieName = ".myAuth"; // TODO: Get it from the cookie settings.
            var cookie = new Cookie(authCookieName, AccessKey); // TODO: Does it need protecting?

            return new ApiClient(Url(relativeApiUrl)).Authenticate(cookie);
        }
    }
}