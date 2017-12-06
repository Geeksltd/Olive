namespace Olive.Services.ImpersonationSession
{
    /// <summary>
    /// Defines an admin user who can impersonate other users.
    /// </summary>
    public interface IImpersonator : IUser, IIdentity, IPrincipal
    {
        /// <summary>
        /// A unique single-use-only cookie-based token to specify the currently impersonated user session.
        /// </summary>
        string ImpersonationToken { get; set; }

        /// <summary>
        /// Determines if this user can impersonate the specified other user.
        /// </summary>
        bool CanImpersonate(IUser user);
    }

    /// <summary>
    /// Provides the business logic for ImpersonationContext class.
    /// </summary>
    public class ImpersonationSession
    {
        /// <summary>
        /// Provides the current user. 
        /// </summary>
        public static Func<IUser> CurrentUserProvider = GetCurrentUser;

        static HttpContext Context => Web.Context.Http;

        /// <summary>
        /// Determines if the current user is impersonated.
        /// </summary>
        public static async Task<bool> IsImpersonated() => await GetImpersonator() != null;

        /// <summary>
        /// Impersonates the specified user by the current admin user.
        /// </summary>
        /// <param name="originalUrl">If not specified, the current HTTP request's URL will be used.</param>
        public static async Task Impersonate(IUser user, bool redirectToHome = true, string originalUrl = null)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var admin = CurrentUserProvider?.Invoke() as IImpersonator;

            if (admin == null)
                throw new InvalidOperationException("The current user is not an IImpersonator.");

            if (!admin.CanImpersonate(user))
                throw new InvalidOperationException("The current user is not allowed to impersonate the specified user.");

            var token = Guid.NewGuid().ToString();

            await Entity.Database.Update(admin, o => o.ImpersonationToken = token);

            SetImpersonationToken(token);

            SetOriginalUrl(originalUrl.Or(Context.Request.ToRawUrl()));

            user.LogOn();

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

            admin.LogOn();

            var returnUrl = await GetOriginalUrl();
            SetOriginalUrl(null);
            SetImpersonationToken(null);

            if (!Context.Request.IsAjaxCall())
                Context.Response.Redirect(returnUrl);
        }

        static IUser GetCurrentUser()
        {
            var result = Context.User as IIdentity;
            if (result == null || !result.IsAuthenticated) return null;

            return result as IUser;
        }

        /// <summary>
        /// Gets the original user who impersonated the current user.
        /// </summary>
        public static async Task<IImpersonator> GetImpersonator()
        {
            var user = CurrentUserProvider?.Invoke();
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