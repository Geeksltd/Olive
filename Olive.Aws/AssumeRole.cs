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

        public static Task Assume(string roleArn)
        {
            RoleArn = roleArn;
            return Renew();
        }

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
                while (true)
                {
                    await Task.Delay(5.Minutes());

                    await SignalRenew();
                }
            }, TaskCreationOptions.LongRunning);
        }

        static async Task Renew()
        {
            var request = new AssumeRoleRequest
            {
                RoleArn = RoleArn,
                DurationSeconds = (int)12.Hours().TotalSeconds
            };

            try
            {
                var response = await TokenService.AssumeRoleAsync(request);
                Log.Debug("AssumeRole response code: " + response.HttpStatusCode);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    LastRenewedUtc = LocalTime.UtcNow;
                    FallbackCredentialsFactory.Reset();
                    FallbackCredentialsFactory.CredentialsGenerators.Insert(0, () => response.Credentials);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Submitting Assume Role request failed.");
                throw;
            }
        }
    }
}