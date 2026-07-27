using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Olive.Entities;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Olive.Security
{
    public class ImpersonationSession
    {
        static HttpContext Context => Olive.Context.Current.Http();
        static IDatabase Database => Olive.Context.Current.Database();

        const string IMPERSONATOR_ROLE = "Olive-IMPERSONATOR";

        static string[] AllowedImpersonators
            => Config.Get("Authentication:Impersonation:Allowed").OrEmpty().Split(',').Trim().ToArray();

        /// <summary>
        /// Determines whether the specified user is permitted to impersonate others, per the comma separated
        /// Authentication:Impersonation:Allowed setting.
        /// </summary>
        /// <remarks>
        /// Denies when that setting is absent. Impersonation is a complete identity takeover, so a missing or
        /// undeployed configuration must not read as "anyone may".
        /// </remarks>
        public static bool CanImpersonate(string email)
            => email.HasValue() &&
               AllowedImpersonators.Any(x => x.Equals(email, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Determines whether the current user is permitted to impersonate others. Use this to decide whether
        /// to offer impersonation in the UI; <see cref="Impersonate(ILoginInfo)"/> enforces it regardless.
        /// </summary>
        public static bool CanCurrentUserImpersonate() => CanImpersonate(Context.User?.GetEmail());

        /// <summary>
        /// Determines if the current user is impersonated.
        /// </summary>
        /// <remarks>
        /// Keyed on the preserved impersonator claims, not on <see cref="IMPERSONATOR_ROLE"/>. The role is
        /// still added by <see cref="Impersonate(ILoginInfo)"/> for apps that read it directly, but it
        /// cannot be relied on: any middleware that rewrites role claims will drop it.
        /// </remarks>
        public static Task<bool> IsImpersonated()
            => Task.FromResult(Context.User.IsImpersonated());

        /// <summary>
        /// Impersonates the specified user by the current admin user.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The current user is not listed in Authentication:Impersonation:Allowed.
        /// </exception>
        public static async Task Impersonate(ILoginInfo user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (!CanCurrentUserImpersonate())
                throw new InvalidOperationException(
                    $"'{Context.User?.GetEmail()}' is not permitted to impersonate. " +
                    "Add them to the Authentication:Impersonation:Allowed setting.");

            user = new GenericLoginInfo
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                ID = user.ID,
                Timeout = user.Timeout,
                Roles = user.GetRoles().Concat(IMPERSONATOR_ROLE).ToArray()
            };

            await user.LogOn(Context.User.ToImpersonatorClaims());

            // Otherwise the impersonator's own JWT lingers and the two cookies name two different people.
            user.SetJwtCookie();
        }

        public static async Task EndImpersonation()
        {
            if (!await IsImpersonated()) throw new InvalidOperationException();


            var principal = Context.User.FromImpersonatorClaims();

            var prop = new AuthenticationProperties
            {
                IsPersistent = Context.User.IsPersistent(),
                ExpiresUtc = Context.User.GetExpiration(),
            };

            await Context.SignOutAsync();
            await Context.SignInAsync(principal, prop);
        }

        public static async Task<string> GetWidget()
        {
            if (!await IsImpersonated()) return string.Empty;
            return "<div class='impersonation-note'>Impersonating <b>" + Context.User.Identity.Name + "</b></div>";
        }
    }
}