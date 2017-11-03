using System;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Olive.Web
{
    public interface IAuthenticationProvider
    {
        Task LogOn(IUser user, string domain, TimeSpan timeout, bool remember);
        Task LogOff(IUser user);
        Task LoginBy(string provider);

        IUser LoadUser(IPrincipal principal);
        void PreRequestHandler(string path);
    }
}
