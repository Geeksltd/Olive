namespace Olive.Security
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
