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

        /// <summary>Adds a configuration for the ApiClient instance used to invoke this Api.</summary>
        public static T Use<T>(this T proxy, Action<ApiClient> config) where T : StronglyTypedApiProxy
            => (T)proxy.Configure(config);

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