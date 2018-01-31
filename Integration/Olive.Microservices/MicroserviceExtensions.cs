using System;
using System.Security.Claims;

namespace Olive
{
    public static class MicroserviceExtensions
    {
        /// <summary>
        /// Determines whether this user is in role TrustedService.
        /// </summary>        
        public static bool IsTrustedService(this ClaimsPrincipal user)
        {
            return user.IsInRole("TrustedService");
        }

        /// <summary>Passes the identity of this service.</summary>
        public static ApiClient AsServiceUser(this ApiClient client)
        {
            if (Microservice.ServiceIdentityCookies.None())
                throw new Exception("This service is not authenticated yet. For ApiClient.AsService() to work " +
                    "you should first authenticate the service in Startup.cs and receive a valid cookie.");

            client.Authenticate(Microservice.ServiceIdentityCookies);
            return client;
        }

        /// <summary>Adds a configuration for the ApiClient instance used to invoke this Api.</summary>
        public static T Use<T>(this T proxy, Action<ApiClient> config) where T : StronglyTypedApiProxy
            => (T)proxy.Configure(config);

        /// <summary>
        /// When calling the remote service, the identity used will be that of the current trusted service as opposed to that of the http context user.
        /// </summary>
        public static T AsServiceUser<T>(this T proxy) where T : StronglyTypedApiProxy
            => proxy.Use(x => x.AsServiceUser());

        /// <summary>
        /// Sets the cache choice for Get requests made by this proxy.
        /// </summary>
        public static T Cache<T>(this T proxy, ApiResponseCache cacheChoice) where T : StronglyTypedApiProxy
        {
            proxy.CacheChoice = cacheChoice;
            return proxy;
        }
    }
}