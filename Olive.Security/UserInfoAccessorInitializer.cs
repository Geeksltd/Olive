using System.Security.Principal;
using Olive.Entities;
using Olive.Web;

namespace Olive.Security
{
    [UserInfoAccessorInitializer]
    public class UserInfoAccessorInitializer
    {
        public static void Initialize() =>
            DefaultApplicationEventManager.InitializeUseAccessors(GetUserPrincipal, GetUserIP);

        static IPrincipal GetUserPrincipal() =>
            Context.HttpContextAccessor.HttpContext.User;

        static string GetUserIP() =>
            Context.HttpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
    }
}
