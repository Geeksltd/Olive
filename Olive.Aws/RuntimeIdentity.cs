using Amazon;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.Aws
{
    /// <summary>
    /// Specifies the current application's runtime IAM identity.
    /// All AWS services, e.g. S3, etc, should use this.
    /// </summary>
    public class RuntimeIdentity
    {
        const string VARIABLE = "AWS_RUNTIME_ROLE_ARN";
        static string RoleArn;

        static IConfiguration Config;
        public static AWSCredentials Credentials { get; private set; }
        static AmazonSecurityTokenServiceClient TokenServiceClient;

        static ILogger Log => Olive.Log.For(typeof(RuntimeIdentity));

        public static async Task Load(IConfiguration config)
        {
            Config = config;
            RoleArn = Environment.GetEnvironmentVariable(VARIABLE);
            Environment.SetEnvironmentVariable(VARIABLE, null);
            TokenServiceClient = new AmazonSecurityTokenServiceClient();

            Log.Info("Runtime role ARN > " + RoleArn);

            await Renew();
            new Thread(KeepRenewing).Start();
        }

        [EscapeGCop("This is a background process")]
        static async void KeepRenewing()
        {
            var interval = Config.GetValue("Aws:Identity:RenewalInterval", 300).Minutes();

            while (true)
            {
                await Task.Delay(interval);
                try
                {
                    await Renew();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to renew AWS credentials.");
                    Environment.Exit(-1);
                }
            }
        }

        static async Task Renew()
        {
            Log.Info("Requesting AssumeRole: " + RoleArn + "...");

            var request = new AssumeRoleRequest
            {
                RoleArn = RoleArn,
                DurationSeconds = (int)12.Hours().TotalSeconds,
                ExternalId = "Pod",
                RoleSessionName = "Pod"
            };

            try
            {
                var response = await TokenServiceClient.AssumeRoleAsync(request);
                Log.Debug("AssumeRole response code: " + response.HttpStatusCode);
                var credentials = response.Credentials;

                FallbackCredentialsFactory.Reset();
                FallbackCredentialsFactory.CredentialsGenerators.Insert(0, () =>
                {
                    Log.Debug("Generating credentials => " + credentials.AccessKeyId.Substring(20) + " of total : " + FallbackCredentialsFactory.CredentialsGenerators.Count);
                    return credentials;
                }
                );

                Log.Debug("Obtained assume role credentials." + credentials.AccessKeyId.Substring(20));

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Submitting Assume Role request failed.");
                throw;
            }
        }
    }
}