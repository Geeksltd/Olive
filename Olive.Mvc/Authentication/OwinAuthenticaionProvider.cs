using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Olive.Web;

namespace Olive.Mvc
{
    public class OwinAuthenticaionProvider : IAuthenticationProvider
    {
        public readonly AsyncEvent<ExternalLoginInfo> ExternalLoginAuthenticated = new AsyncEvent<ExternalLoginInfo>();
        // public string AuthenticationScheme { get; }

        // public OwinAuthenticaionProvider(string authenticationScheme) => AuthenticationScheme = authenticationScheme;

        public async Task LogOn(IUser user, string domain, TimeSpan timeout, bool remember)
        {
            var context = Context.Http;

            await context.SignOutAsync(IdentityConstants.ApplicationScheme);

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.GetId().ToString())
            };

            claims.AddRange(user.GetRoles().Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));

            await context.SignInAsync(IdentityConstants.ApplicationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, "AuthenticationType")), // AuthenticationType is just a text and I do not know what is its usage.
                new AuthenticationProperties
                {
                    IsPersistent = remember,
                    ExpiresUtc = DateTimeOffset.UtcNow.Add(timeout)
                });
        }

        public Task LogOff(IUser user) => Context.Http.SignOutAsync(IdentityConstants.ApplicationScheme);

        public async Task LoginBy(string provider)
        {
            if (Context.HttpContextAccessor.HttpContext.Request.Query["ReturnUrl"].ToString().IsEmpty())
            {
                // it's mandatory, otherwise Challenge() immediately returns to Login page
                throw new InvalidOperationException("Request has no ReturnUrl.");
            }

            await Context.Http.ChallengeAsync(provider, new AuthenticationProperties {
                RedirectUri = "/ExternalLoginCallback",
                Items = { new KeyValuePair<string, string>("LoginProvider", provider) }
            });
        }

        IUser IAuthenticationProvider.LoadUser(IPrincipal principal)
        {
            throw new NotSupportedException("IAuthenticationProvider.LoadUser() is deprecated in M# MVC.");
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