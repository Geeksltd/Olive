using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Olive.Entities;
using Olive.Web;

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

    /// <summary>
    /// Provides the business logic for ImpersonationContext class.
    /// </summary>
    public class ImpersonationSession
    {
        static HttpContext Context => Olive.Context.Current.Http();

        /// <summary>
        /// Determines if the current user is impersonated.
        /// </summary>
        public static async Task<bool> IsImpersonated() => await GetImpersonator() != null;

        /// <summary>
        /// Impersonates the specified user by the current admin user.
        /// </summary>
        /// <param name="originalUrl">If not specified, the current HTTP request's URL will be used.</param>
        public static async Task Impersonate(ILoginInfo user, bool redirectToHome = true, string originalUrl = null)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var admin = GetCurrentUser() as IImpersonator
                ?? throw new InvalidOperationException("The current user is not an IImpersonator.");

            if (!admin.CanImpersonate(user))
                throw new InvalidOperationException("The current user is not allowed to impersonate the specified user.");

            var token = Guid.NewGuid().ToString();

            await Entity.Database.Update(admin, o => o.ImpersonationToken = token);

            SetImpersonationToken(token);

            SetOriginalUrl(originalUrl.Or(Context.Request.ToRawUrl()));

            await user.LogOn();

            if (redirectToHome && !Context.Request.IsAjaxCall())
                Context.Response.Redirect("~/");
        }

        /// <summary>
        /// Ends the current impersonation session.
        /// </summary>
        public static async Task End()
        {
            if (!await IsImpersonated()) return;

            var admin = await GetImpersonator();

            await Entity.Database.Update(admin, o => o.ImpersonationToken = null);

            await admin.LogOn();

            var returnUrl = await GetOriginalUrl();
            SetOriginalUrl(null);
            SetImpersonationToken(null);

            if (!Context.Request.IsAjaxCall())
                Context.Response.Redirect(returnUrl);
        }

        static IPrincipal GetCurrentUser()
        {
            var result = Context.User as IPrincipal;
            if (result == null || !result.Identity.IsAuthenticated) return null;

            return result;
        }

        /// <summary>
        /// Gets the original user who impersonated the current user.
        /// </summary>
        public static async Task<IImpersonator> GetImpersonator()
        {
            var user = GetCurrentUser();
            if (user == null || user.IsInRole("Guest") || user.IsInRole("Anonymous")) return null;

            var token = await ImpersonationToken;
            if (token.IsEmpty()) return null;

            return await Entity.Database.FirstOrDefault<IImpersonator>(x => x.ImpersonationToken == token);
        }

        static Task<string> ImpersonationToken => CookieProperty.Get("Impersonation.Token");

        static void SetImpersonationToken(string value) => CookieProperty.Set("Impersonation.Token", value);

        public static async Task<string> GetOriginalUrl() => (await CookieProperty.Get("Impersonation.Original.Url")).Or("~/");

        public static void SetOriginalUrl(string value) => CookieProperty.Set("Impersonation.Original.Url", value);
    }
}