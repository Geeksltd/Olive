using System;
using System.Linq;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Olive.Security;
using Olive.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

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
            var securityKey = Config.Get("Authentication:JWT:Secret").ToBytes();

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = info.ToClaimsIdentity(),
                Issuer = Context.Request.GetWebsiteRoot(),
                Audience = Context.Request.GetWebsiteRoot(),
                Expires = DateTime.UtcNow.Add(info.Timeout),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(securityKey), SecurityAlgorithms.HmacSha256),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(descriptor);
            return tokenHandler.WriteToken(token);
        }

        public static string GetEmail(this ClaimsPrincipal principal) => principal.FindFirstValue(ClaimTypes.Email);

        public static string GetId(this ClaimsPrincipal principal) => principal.FindFirstValue(ClaimTypes.NameIdentifier);

        public static IEnumerable<string> GetRoles(this ClaimsPrincipal principal)
        => principal.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).Trim();

        public static string GetFirstIssuer(this ClaimsPrincipal principal)
        {
            return principal?.Claims?.Select(x => x.Issuer).Trim().FirstOrDefault();
        }

        public static async Task LogOn(this ILoginInfo loginInfo, bool remember = false)
        {
            await Context.Http.SignOutAsync();

            var prop = new AuthenticationProperties
            {
                IsPersistent = remember,
                ExpiresUtc = DateTimeOffset.UtcNow.Add(loginInfo.Timeout)
            };

            await Context.Http.SignInAsync(new ClaimsPrincipal(loginInfo.ToClaimsIdentity()), prop);
        }
    }
}