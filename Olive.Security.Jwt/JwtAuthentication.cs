namespace Olive.Security
{
    public static class JwtAuthentication
    {
        static JwtEncoder JwtEncoder;
        static JwtDecoder JwtDecoder;

        public static string CreateTicket(IUser user, DateTime? expiryDate = null)
        {
            var token = new { ID = user.GetId(), Expiry = expiryDate?.ToString() };

            JwtEncoder = JwtEncoder ?? new JwtEncoder(
                                        new RS256Algorithm(new X509Certificate2()),
                                        new JsonNetSerializer(),
                                        new JwtBase64UrlEncoder()
                                        );

            return JwtEncoder.Encode(token, Config.Get("JWT.Token.Secret"));
        }

        public static string ExtractUserId(HttpRequestHeaders headers)
        {
            if (headers.Authorization?.Scheme != "Bearer") return null;

            try
            {
                var jwt = headers.Authorization.Parameter;

                if (jwt.IsEmpty()) return null;

                JwtDecoder = JwtDecoder ?? new JwtDecoder(
                                            new JsonNetSerializer(),
                                            new JwtValidator(new JsonNetSerializer(), new UtcDateTimeProvider()),
                                            new JwtBase64UrlEncoder()
                                            );

                var values = JwtDecoder.DecodeToObject(jwt, Config.Get("JWT.Token.Secret"), verify: true) as IDictionary<string, object>;

                var expiry = values["Expiry"].ToStringOrEmpty().TryParseAs<DateTime>();

                if (expiry <= LocalTime.Now) return null;

                return values["ID"].ToStringOrEmpty();
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

        public static Task<IUser> ExtractUser(HttpRequestHeaders headers)
        {
            var userId = ExtractUserId(headers);

            if (userId.IsEmpty()) return null;
            return Database.Instance.GetOrDefault<IUser>(userId);
        }
    }
}