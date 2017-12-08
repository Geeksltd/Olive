using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Olive.Web;

namespace Olive.Security
{
    public class OAuth
    {
        public readonly static OAuth Instance = new OAuth();

        public readonly AsyncEvent<ExternalLoginInfo> ExternalLoginAuthenticated = new AsyncEvent<ExternalLoginInfo>();

        static ClaimsIdentity CreateIdentity(string displayName, string userId, string email, IEnumerable<string> roles, TimeSpan timeout)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, displayName) };

            if (userId.HasValue()) claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            if (email.HasValue()) claims.Add(new Claim(ClaimTypes.Email, userId));

            foreach (var role in roles.OrEmpty())
                claims.Add(new Claim(ClaimTypes.Role, role));

            return new ClaimsIdentity(new GenericIdentity(displayName), claims);
        }

        public async Task LogOn(string displayName, string userId, string email,
            IEnumerable<string> roles, TimeSpan timeout, bool remember)
        {
            await Context.Http.SignOutAsync();

            var identity = CreateIdentity(displayName, userId, email, roles, timeout);

            var prop = new AuthenticationProperties
            {
                IsPersistent = remember,
                ExpiresUtc = DateTimeOffset.UtcNow.Add(timeout)
            };

            await Context.Http.SignInAsync(new ClaimsPrincipal(identity), prop);
        }

        public async Task LogOff(IIdentity user)
        {
            await Context.Http.SignOutAsync();
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

        public static string CreateJwtToken(string displayName, string userId, string email, IEnumerable<string> roles, TimeSpan timeout)
        {
            var securityKey = Config.Get("JWT.Token.Secret").ToBytes();

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = OAuth.CreateIdentity(displayName, userId, email, roles, timeout),
                Issuer = Context.Request.GetWebsiteRoot(),
                Audience = Context.Request.GetWebsiteRoot(),
                Expires = DateTime.UtcNow.Add(timeout),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(securityKey), SecurityAlgorithms.HmacSha256),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(descriptor);
            return tokenHandler.WriteToken(token);
        }

        public static ClaimsPrincipal DecodeJwt(string jwt)
        {
            if (jwt.IsEmpty()) return null;
            return new JwtSecurityTokenHandler().ValidateToken(jwt, new TokenValidationParameters(), out var token);
        }
    }
}