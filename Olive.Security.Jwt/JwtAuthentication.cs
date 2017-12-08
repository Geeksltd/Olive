namespace Olive.Security
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using JWT;
    using JWT.Algorithms;
    using JWT.Serializers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.IdentityModel.Tokens;
    using Olive.Web;

    public class JwtAuthentication
    {
        static JwtEncoder encoder;
        static JwtDecoder decoder;

        static JwtEncoder Encoder
        {
            get
            {
                if (encoder == null)
                    encoder = new JwtEncoder(
                                          new RS256Algorithm(new X509Certificate2()),
                                          new JsonNetSerializer(),
                                          new JwtBase64UrlEncoder());

                return encoder;
            }
        }

        static JwtDecoder Decoder
        {
            get
            {
                if (decoder == null)
                    decoder = new JwtDecoder(
                                              new JsonNetSerializer(),
                                              new JwtValidator(new JsonNetSerializer(), new UtcDateTimeProvider()),
                                              new JwtBase64UrlEncoder()
                                              );
                return decoder;
            }
        }

        public static string CreateTicket(IIdentity user, IEnumerable<string> roles, DateTime? expiryDate = null)
        {
           // TODO: MAKE It the same API as the OAuth.Logon!!!!!!!!!!!!

           var token = new
           {
               Name = user.Name,
               Roles = roles.ToString("|"),
               Expiry = expiryDate?.ToString()
           };

            return Encoder.Encode(token, Config.Get("JWT.Token.Secret"));
        }

        internal static ClaimsPrincipal ExtractUser(IHeaderDictionary headers)
        {
            var headerVlaue = headers["Authorization"].ToString();
            // Checking the scheme
            if (!headerVlaue.StartsWith("Bearer")) return null;

            try
            {
                var jwt = headerVlaue.TrimStart("Bearer").TrimStart();
                if (jwt.IsEmpty()) return null;

                var result = new JwtSecurityTokenHandler().ValidateToken(jwt, new TokenValidationParameters(), out var token);
                return result;

            }
            catch (ArgumentException)
            {
                return null; // invalid JWT format
            }
            catch (SignatureVerificationException)
            {
                return null;
            }
        }
    }
}