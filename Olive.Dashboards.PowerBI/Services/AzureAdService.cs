namespace Olive.Dashboards.PowerBI.Services
{
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Client;
    using Olive.Dashboards.PowerBI.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using System.Text;
    using System.Threading.Tasks;

    public class AzureAdService
    {
        private readonly IOptions<AzureAd> AzureAd;

        public AzureAdService(IOptions<AzureAd> azureAd)
        {
            AzureAd = azureAd;
        }

        /// <summary>
        /// Generates and returns Access token
        /// </summary>
        /// <returns>AAD token</returns>
        public async Task<string> GetAccessToken()
        {
            AuthenticationResult authenticationResult = null;

            if (AzureAd.Value.AuthenticationMode.Equals("masteruser", StringComparison.InvariantCultureIgnoreCase))
            {
                // Create a public client to authorize the app with the AAD app
                var clientApp = PublicClientApplicationBuilder
                    .Create(AzureAd.Value.ClientId)
                    .WithAuthority(AzureAd.Value.AuthorityUri).Build();

                var userAccounts = await clientApp.GetAccountsAsync();

                try
                {
                    // Retrieve Access token from cache if available
                    authenticationResult = await clientApp
                        .AcquireTokenSilent(AzureAd.Value.Scope, userAccounts.FirstOrDefault())
                        .ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    var password = new SecureString();

                    foreach (var key in AzureAd.Value.PbiPassword)
                    {
                        password.AppendChar(key);
                    }

                    authenticationResult = await clientApp
                        .AcquireTokenByUsernamePassword(AzureAd.Value.Scope, AzureAd.Value.PbiUsername, password)
                        .ExecuteAsync();
                }
            }

            // Service Principal auth is the recommended by Microsoft to achieve App Owns Data Power BI embedding
            else if (AzureAd.Value.AuthenticationMode.Equals("serviceprincipal", StringComparison.InvariantCultureIgnoreCase))
            {
                // For app only authentication, we need the specific tenant id in the authority url
                var tenantSpecificUrl = AzureAd.Value.AuthorityUri.Replace("organizations", AzureAd.Value.TenantId);

                // Create a confidential client to authorize the app with the AAD app
                var clientApp = ConfidentialClientApplicationBuilder.Create(AzureAd.Value.ClientId)
                    .WithClientSecret(AzureAd.Value.ClientSecret)
                    .WithAuthority(tenantSpecificUrl)
                    .Build();

                // Make a client call if Access token is not available in cache
                authenticationResult = await clientApp.AcquireTokenForClient(AzureAd.Value.Scope).ExecuteAsync();
            }

            return authenticationResult.AccessToken;
        }
    }
}
