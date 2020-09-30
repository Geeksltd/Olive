using Microsoft.AspNetCore.Authentication;
using Olive.Security;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

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

        public static string CreateJwtToken(this ILoginInfo @this)
        {
            var securityKey = OAuth.GetJwtSecurityKey();

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = @this.ToClaimsIdentity(),
                Issuer = Context.Current.Request().RootUrl(),
                Audience = Context.Current.Request().RootUrl(),
                Expires = DateTime.UtcNow.Add(@this.Timeout ?? DistantFuture),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(descriptor);
            return tokenHandler.WriteToken(token);
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