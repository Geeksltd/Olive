using Olive.Entities;

namespace Olive.Security.Impersonation
{
    /// <summary>
    /// Defines an admin user who can impersonate other users.
    /// </summary>
    public interface IImpersonator : ILoginInfo, IEntity
    {
        /// <summary>
        /// A unique single-use-only cookie-based token to specify the currently impersonated user session.
        /// </summary>
        string ImpersonationToken { get; set; }

        /// <summary>
        /// Determines if this user can impersonate the specified other user.
        /// </summary>
        bool CanImpersonate(ILoginInfo user);
    }
}