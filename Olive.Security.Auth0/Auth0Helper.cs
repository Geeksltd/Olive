using System;
using System.Threading.Tasks;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.Core.Exceptions;
using Olive;

namespace Olive.Security.Auth0
{
    public class Auth0Helper
    {
        static readonly AuthenticationApiClient ApiClient;

        static string ClientId => Config.Get("Authentication:Auth0:ClientId");
        static string ClientSecret => Config.Get("Authentication:Auth0:ClientSecret");
        static Uri ServiceUrl => new Uri($"https://{Config.Get("Authentication:Auth0:Domain")}/");

        static Auth0Helper()
        {
            ApiClient = new AuthenticationApiClient(ServiceUrl);
        }

        public static async Task<AuthenticationResult> Authenticate(string username, string password)
        {
            var request = new UsernamePasswordLoginRequest
            {
                ClientId = ClientId,
                Connection = "Username-Password-Authentication",
                Scope = "openid profile",
                Username = username,
                Password = password
            };

            try
            {
                await ApiClient.UsernamePasswordLoginAsync(request);
                // No error:
                return new AuthenticationResult() { Success = true };
            }
            catch (ApiException exception)
            {
                return new AuthenticationResult() { Message = exception.Message };
            }
        }

        public class AuthenticationResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }
    }
}