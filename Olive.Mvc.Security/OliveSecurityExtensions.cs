using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Olive.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Olive
{
    public static class OliveSecurityExtensions
    {
        readonly static TimeSpan DistantFuture = 10000.Days();

        public static ClaimsIdentity ToClaimsIdentity(this ILoginInfo @this)
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, @this.DisplayName.OrEmpty()),
                new Claim(ClaimTypes.Expiration, DateTimeOffset.UtcNow.Add(@this.Timeout ?? DistantFuture).ToString()),
            };

            if (@this.ID.HasValue()) claims.Add(new Claim(ClaimTypes.NameIdentifier, @this.ID));
            if (@this.Email.HasValue()) claims.Add(new Claim(ClaimTypes.Email, @this.Email));

            var roles = @this.GetRoles().OrEmpty().ToString(",");

            if (roles.HasValue())
                claims.Add(new Claim(ClaimTypes.Role, roles));

            return new ClaimsIdentity(claims, "Olive");
        }

        public static string CreateJwtToken(this ILoginInfo @this, IEnumerable<Claim> additionalClaims = null, bool remember = false)
        {
            var securityKey = OAuth.GetJwtSecurityKey();

            var identity = @this.ToClaimsIdentity();
            identity.AddClaims(additionalClaims.OrEmpty());
            identity.AddClaim(new Claim(ClaimTypes.IsPersistent, remember.ToString()));

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Issuer = Context.Current.Request().RootUrl(),
                Audience = Context.Current.Request().RootUrl(),
                Expires = DateTime.UtcNow.Add(@this.Timeout ?? DistantFuture),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256),
            };

            var tokenHandler = new JsonWebTokenHandler();
            string token = tokenHandler.CreateToken(descriptor);
            return token;
        }

        public static async Task LogOn(this ILoginInfo @this, IEnumerable<Claim> additionalClaims = null, bool remember = false)
        {
            var prop = new AuthenticationProperties
            {
                IsPersistent = remember,
                ExpiresUtc = DateTimeOffset.UtcNow.Add(@this.Timeout ?? DistantFuture)
            };

            var identity = @this.ToClaimsIdentity();
            identity.AddClaims(additionalClaims.OrEmpty());
            identity.AddClaim(new Claim(ClaimTypes.IsPersistent, remember.ToString()));

            await Context.Current.Http().SignInAsync(new ClaimsPrincipal(identity), prop);
        }

        /// <summary>
        /// Writes this login's JWT into the configured cookie, so javascript and API clients identify the
        /// same user as the auth cookie does. Does nothing unless Authentication:JWT:Cookie:Name and a
        /// cookie domain are configured.
        /// </summary>
        /// <remarks>
        /// If the token cannot be issued, any existing cookie is removed rather than left alone: a stale JWT
        /// naming the previous user is worse than no JWT at all.
        /// </remarks>
        public static void SetJwtCookie(this ILoginInfo @this, bool remember = false)
        {
            var cookieName = Config.Get("Authentication:JWT:Cookie:Name");
            if (cookieName.IsEmpty()) return;

            var domain = Config.Get("Authentication:JWT:Cookie:Domain").Or(Config.Get("Authentication:Cookie:Domain"));
            if (domain.IsEmpty()) return;

            var response = Context.Current.Http().Response;

            try
            {
                response.Cookies.Append(cookieName, @this.CreateJwtToken(remember: remember), new CookieOptions
                {
                    Domain = domain,
                    MaxAge = @this.Timeout,
                    Secure = Context.Current.Request().IsHttps,
                    HttpOnly = false,
                    SameSite = SameSiteMode.Lax
                });
            }
            catch (Exception ex)
            {
                Log.For(typeof(OliveSecurityExtensions))
                    .Error(ex, $"Failed to issue a JWT for {@this.Email}; removing the existing one.");

                response.Cookies.Delete(cookieName, new CookieOptions { Domain = domain });
            }
        }

        public static bool IsPersistent(this ClaimsPrincipal @this)
            => @this?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.IsPersistent)?.Value?.To<bool>() ?? false;

        public static DateTimeOffset GetExpiration(this ClaimsPrincipal @this)
        {
            var temp = @this?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Expiration)?.Value;

            if (DateTimeOffset.TryParse(temp, out var result))
                return result;
            else
                return DateTimeOffset.MaxValue;
        }

        /// <summary>
        /// Determines whether the ID of this user is the same as a specified loggin-in user.
        /// </summary>
        public static bool Is(this ILoginInfo @this, ClaimsPrincipal loggedInUser)
            => loggedInUser.GetId() == @this.ID;

        /// <summary>
        /// Determines whether the ID of this logged-in user is the same as a specified user.
        /// </summary>
        public static bool Is(this ClaimsPrincipal @this, ILoginInfo loginInfo) => loginInfo.Is(@this);

        public static GenericLoginInfo Clone(this ILoginInfo @this, Action<GenericLoginInfo> change = null)
        {
            var clone = new GenericLoginInfo(@this);
            change?.Invoke(clone);
            return clone;
        }
    }
}