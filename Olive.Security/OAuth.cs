using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Security
{
    public class OAuth
    {
        const int JWT_SECRET_LENGTH = 21;

        public readonly static OAuth Instance = new OAuth();

        public event AwaitableEventHandler<ExternalLoginInfo> ExternalLoginAuthenticated;

        public async Task LogOff()
        {
            var http = Context.Current.Http();
            if (http != null) await http.SignOutAsync();
            Context.Current.GetOptionalService<ISession>()?.Clear();
        }

        public async Task LoginBy(string provider)
        {
            if (Context.Current.Request().Param("ReturnUrl").IsEmpty())
            {
                // it's mandatory, otherwise Challenge() immediately returns to Login page
                throw new InvalidOperationException("Request has no ReturnUrl.");
            }

            await Context.Current.Http().ChallengeAsync(provider, new AuthenticationProperties
            {
                RedirectUri = "/ExternalLoginCallback",
                Items = { new KeyValuePair<string, string>("LoginProvider", provider) }
            });
        }

        public async Task NotifyExternalLoginAuthenticated(ExternalLoginInfo info)
        {
            if (ExternalLoginAuthenticated is null)
                throw new InvalidOperationException("ExternalLogin requested but no handler found for ExternalLoginAuthenticated event");

            await ExternalLoginAuthenticated.Raise(info);
        }

        public ClaimsPrincipal DecodeJwt(string jwt)
        {
            if (jwt.IsEmpty()) return null;

            var validationParams = new TokenValidationParameters
            {
                IssuerSigningKey = GetJwtSecurityKey(),
                ValidateAudience = false,
                ValidateIssuer = false
            };

            return new JwtSecurityTokenHandler().ValidateToken(jwt, validationParams, out var token);
        }

        internal static SymmetricSecurityKey GetJwtSecurityKey()
        {
            var configKey = Config.GetOrThrow("Authentication:JWT:Secret");
            if (configKey.Length != JWT_SECRET_LENGTH)
                throw new ArgumentException("Your config setting of 'Authentication:JWT:Secret' needs to be 21 characters.");

            var securityKey = configKey.ToBytes(encoding: Encoding.UTF8);
            return new SymmetricSecurityKey(securityKey);
        }
    }
}