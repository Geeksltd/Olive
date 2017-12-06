using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Olive.Web;

namespace Olive.Security
{
    public class OAuth
    {
        public readonly static OAuth Instance = new OAuth();

        public readonly AsyncEvent<ExternalLoginInfo> ExternalLoginAuthenticated = new AsyncEvent<ExternalLoginInfo>();

        public async Task LogOn(IIdentity user, IEnumerable<string> roles, TimeSpan timeout, bool remember, string domain = null)
        {
            var context = Context.Http;

            await context.SignOutAsync(IdentityConstants.ApplicationScheme);

            await context.SignInAsync(IdentityConstants.ApplicationScheme,
                user.CreateClaimsPrincipal(roles, "OAuth"),
                new AuthenticationProperties
                {
                    IsPersistent = remember,
                    ExpiresUtc = DateTimeOffset.UtcNow.Add(timeout)
                });
        }

        public async Task LogOff(IIdentity user)
        {
            await Context.Http.SignOutAsync(IdentityConstants.ApplicationScheme);
            Context.Http.Session.Perform(s => s.Clear());
        }

        public async Task LoginBy(string provider)
        {
            if (Context.HttpContextAccessor.HttpContext.Request.Query["ReturnUrl"].ToString().IsEmpty())
            {
                // it's mandatory, otherwise Challenge() immediately returns to Login page
                throw new InvalidOperationException("Request has no ReturnUrl.");
            }

            await Context.Http.ChallengeAsync(provider, new AuthenticationProperties
            {
                RedirectUri = "/ExternalLoginCallback",
                Items = { new KeyValuePair<string, string>("LoginProvider", provider) }
            });
        }

        public void PreRequestHandler(string path)
        {
            if (path == "/ExternalLoginCallback")
            {
                // this needs to be done here (PreRequestHandler) because we need to get owin context from httpcontext
                ExternalLoginCallback();
            }
        }

        internal void ExternalLoginCallback()
        {
            throw new NotImplementedException("The following code is commented to fix on the test time.");
            // var authenticationManager = AccessorsHelper.HttpContextAccessor.HttpContext.GetOwinContext().Authentication;
            // var loginInfo = authenticationManager.GetExternalLoginInfo();

            // var info = new ExternalLoginInfo();

            // if (loginInfo != null)
            // {
            //    var nameIdentifierClaim = loginInfo.ExternalIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            //    info.IsAuthenticated = loginInfo.ExternalIdentity.IsAuthenticated;
            //    info.Issuer = nameIdentifierClaim.Get(c => c.Issuer);
            //    info.NameIdentifier = nameIdentifierClaim.Get(c => c.Value);
            //    info.Email = loginInfo.Email;
            //    info.UserName = loginInfo.DefaultUserName;
            // }

            // NotifyExternalLoginAuthenticated(this, info);
        }

        public async Task NotifyExternalLoginAuthenticated(ExternalLoginInfo info)
        {
            if (!ExternalLoginAuthenticated.IsHandled())
                throw new InvalidOperationException("ExternalLogin requested but no handler found for ExternalLoginAuthenticated event");

            await ExternalLoginAuthenticated.Raise(info);
        }
    }
}