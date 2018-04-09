using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using Olive.Security;

namespace Olive
{
    public static class OliveSecurityExtensions
    {
        public static ClaimsIdentity ToClaimsIdentity(this ILoginInfo info)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, info.DisplayName.OrEmpty()) };

            if (info.ID.HasValue()) claims.Add(new Claim(ClaimTypes.NameIdentifier, info.ID));
            if (info.Email.HasValue()) claims.Add(new Claim(ClaimTypes.Email, info.Email));

            foreach (var role in info.GetRoles().OrEmpty())
                claims.Add(new Claim(ClaimTypes.Role, role));

            return new ClaimsIdentity(claims, "Olive");
        }

        public static string CreateJwtToken(this ILoginInfo info)
        {
            var securityKey = OAuth.GetJwtSecurityKey();

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = info.ToClaimsIdentity(),
                Issuer = Context.Current.Request().RootUrl(),
                Audience = Context.Current.Request().RootUrl(),
                Expires = DateTime.UtcNow.Add(info.Timeout ?? 10000.Days()),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(descriptor);
            return tokenHandler.WriteToken(token);
        }

        public static async Task LogOn(this ILoginInfo loginInfo, bool remember = false)
        {
            await Context.Current.Http().SignOutAsync();

            var prop = new AuthenticationProperties
            {
                IsPersistent = remember,
                ExpiresUtc = DateTimeOffset.UtcNow.Add(loginInfo.Timeout ?? 10000.Days())
            };

            await Context.Current.Http().SignInAsync(new ClaimsPrincipal(loginInfo.ToClaimsIdentity()), prop);
        }

        /// <summary>
        /// Determines whether the ID of this user is the same as a specified loggin-in user.
        /// </summary>
        public static bool Is(this ILoginInfo loginInfo, ClaimsPrincipal loggedInUser)
            => loggedInUser.GetId() == loginInfo.ID;

        /// <summary>
        /// Determines whether the ID of this logged-in user is the same as a specified user.
        /// </summary>
        public static bool Is(this ClaimsPrincipal loggedInUser, ILoginInfo loginInfo) => loginInfo.Is(loggedInUser);

        public static GenericLoginInfo Clone(this ILoginInfo info, Action<GenericLoginInfo> change = null)
        {
            var clone = new GenericLoginInfo(info);
            change?.Invoke(clone);
            return clone;
        }
    }
}