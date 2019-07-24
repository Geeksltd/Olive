using Amazon;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Olive;
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
        static AWSCredentials DefaultCredentials;

        static ILogger Log => Olive.Log.For(typeof(RuntimeIdentity));

        public static async Task Load(IConfiguration config)
        {
            Config = config;
            RoleArn = Environment.GetEnvironmentVariable(VARIABLE);
            Environment.SetEnvironmentVariable(VARIABLE, null);
            DefaultCredentials = GetDefaultCredentials();

            Log.Info("Runtime role ARN > " + RoleArn);

            await Renew();
            new Thread(KeepRenewing).Start();
        }

        private static AWSCredentials GetDefaultCredentials()
        {
            foreach (var provider in FallbackCredentialsFactory.CredentialsGenerators)
            {
                try
                {
                    Console.WriteLine($"Loading Credentials for {provider}.");
                    return provider.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load Credentials for {provider} because : " + ex.ToFullMessage());
                }
            }

            throw new Exception("Could not load the default credentials.");
        }

        [EscapeGCop("This is a background process")]
        static async void KeepRenewing()
        {
            while (true)
            {
                await Task.Delay(5.Hours());
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

        internal static async Task Renew()
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
                using (var client = new AmazonSecurityTokenServiceClient(DefaultCredentials))
                {
                    var response = await client.AssumeRoleAsync(request);
                    Log.Debug("AssumeRole response code: " + response.HttpStatusCode);
                    var credentials = response.Credentials;

                    FallbackCredentialsFactory.Reset();
                    FallbackCredentialsFactory.CredentialsGenerators.Insert(0, () => credentials);

                    Log.Debug("Obtained assume role credentials.");
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