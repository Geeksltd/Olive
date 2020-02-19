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

        /// <summary>
        /// Determines if the current user is impersonated.
        /// </summary>
        public static Task<bool> IsImpersonated()
            => Task.FromResult(Context.User.IsInRole(IMPERSONATOR_ROLE));

        /// <summary>
        /// Impersonates the specified user by the current admin user.
        /// </summary> 
        public static async Task Impersonate(ILoginInfo user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            user = new GenericLoginInfo
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                ID = user.ID,
                Timeout = user.Timeout,
                Roles = user.GetRoles().Concat(IMPERSONATOR_ROLE).ToArray()
            };

            await user.LogOn(Context.User.ToImpersonatorClaims());
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