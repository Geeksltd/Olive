using System;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Microsoft.Extensions.Logging;

namespace Olive.Aws
{
    public static class AssumeRole
    {
        static Task RenewingTask;
        static ILogger Log => Olive.Log.For(typeof(AssumeRole));
        static AmazonSecurityTokenServiceClient TokenService = new AmazonSecurityTokenServiceClient();
        static DateTime LastRenewedUtc;
        static string RoleArn;

        /// <summary>
        /// This will change the default application identity to the specified role in the specified AWS account.
        /// </summary>
        public static Task Assume(string roleArn)
        {
            RoleArn = roleArn;
            return Renew();
        }

        /// <summary>
        /// This will create the credentials to use for creating new AWS clients with the identify of the specified role.
        /// </summary>
        public static Task<Credentials> Temporary(string roleArn) => LoginAs(roleArn);

        public static async Task SignalRenew()
        {
            if (LocalTime.UtcNow.Subtract(LastRenewedUtc) > 5.Minutes())
            {
                try { await Renew(); }
                catch
                {
                    // No logging needed
                }
            }
        }

        [EscapeGCop("This is a background process")]
        public static void KeepRenewing()
        {
            if (RenewingTask != null) return;

            RenewingTask = Task.Factory.StartNew(async () =>
            {
                while (true) await Task.Delay(5.Minutes()).Then(SignalRenew);
            }, TaskCreationOptions.LongRunning);
        }

        static async Task Renew()
        {
            var cred = await LoginAs(RoleArn);

            LastRenewedUtc = LocalTime.UtcNow;
            FallbackCredentialsFactory.Reset();
            FallbackCredentialsFactory.CredentialsGenerators.Insert(0, () => cred);
        }

        static async Task<Credentials> LoginAs(string roleArn)
        {
            var request = new AssumeRoleRequest
            {
                RoleArn = roleArn,
                ExternalId = "Temporary",
                RoleSessionName = "Temporary"
            };

            try
            {
                var response = await TokenService.AssumeRoleAsync(request);
                Log.Debug("AssumeRole response code: " + response.HttpStatusCode);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return response.Credentials;
                }
                throw new Exception("AssumeRole failed: " + response.HttpStatusCode);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Submitting Assume Role request failed.");
                throw;
            }
        }
    }
}