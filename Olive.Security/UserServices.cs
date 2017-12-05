using Olive.Web;
using System;
using System.ComponentModel;

namespace Olive.Security
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class UserServices
    {
        const int DEFAULT_AUTHENTICATION_TIMEOUT_IN_MINUTES = 20;

        public static IAuthenticationProvider AuthenticationProvider;

        static UserServices()
        {
            var provider = Config.Get("Authentication:Provider");
            if (provider.HasValue())
                AuthenticationProvider = (IAuthenticationProvider)Type.GetType(provider).CreateInstance();
            else
                throw new Exception("The authentication provider is not specified.");
        }

        public static void LogOn(this IUser user) => LogOn(user, domain: null, remember: false);

        static void LogOn(this IUser user, string domain) => LogOn(user, domain, remember: false);

        public static void LogOn(this IUser user, bool remember) => LogOn(user, null, remember);

        public static TimeSpan GetAuthenticationTimeout()
        {
            try
            {
                var minutes = Config.Get("Authentication:Timeout", 5);

                return TimeSpan.FromMinutes(minutes);
            }
            catch
            {
                // No Logging Neede
                return TimeSpan.FromMinutes(DEFAULT_AUTHENTICATION_TIMEOUT_IN_MINUTES);
            }
        }

        static void LogOn(this IUser user, string domain, bool remember) =>
            AuthenticationProvider.LogOn(user, domain, GetAuthenticationTimeout(), remember);

        public static void LogOff(this IUser user)
        {
            AuthenticationProvider.LogOff(user);

            Context.HttpContextAccessor.HttpContext.Session.Perform(s => s.Clear());
        }

        public static void LoginBy(string provider) => AuthenticationProvider.LoginBy(provider);
    }
}