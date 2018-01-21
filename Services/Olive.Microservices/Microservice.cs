using System;
using Olive.Web;

namespace Olive
{
    /// <summary>
    /// Provides helper services for implementing microservices using Olive.
    /// </summary>
    public class Microservice
    {
        /// <summary>
        /// Gets the value of the config key Microservice:Root.Domain.
        /// </summary>
        public static string RootDomain => Config.Get("Microservice:Root.Domain");

        /// <summary>
        /// Gets 'http'or 'https' baesd on the curretn config value of Microservice:Http.Protocol.
        /// </summary>
        public static string HttpProtocol => Config.Get("Microservice:Http.Protocol");

        /// <summary>
        /// For example for the specified service name of 'auth' 
        /// it will return the current full url such as https://auth.my-services.com.
        /// </summary>
        public static string Url(string serviceName, string relativeUrl = null)
        {
            return HttpProtocol + "://" + serviceName + "." + RootDomain + relativeUrl.EnsureStartsWith("/");
        }
    }
}