using Amazon;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
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

        static readonly string RegionName;
        static string RoleArn;

        public static AWSCredentials Credentials { get; private set; }

        static RuntimeIdentity()
        {
            RoleArn = Environment.GetEnvironmentVariable(VARIABLE);
            Environment.SetEnvironmentVariable(VARIABLE, null);

            RegionName = Config.Get("Aws:Region",
                Environment.GetEnvironmentVariable("AWS_RUNTIME_ROLE_REGION"));
        }

        public static RegionEndpoint Region => RegionEndpoint.GetBySystemName(RegionName);

        internal static async Task Load()
        {
            await Renew();

            new Thread(KeepRenewing).Start();
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
                    Log.For<RuntimeIdentity>().Error(ex, "Failed to renew AWS credentials.");
                    Environment.Exit(-1);
                }
            }
        }

        static async Task Renew()
        {
            var request = new AssumeRoleRequest
            {
                RoleArn = RoleArn,
                DurationSeconds = (int)12.Hours().TotalSeconds,
                ExternalId = "Pod",
                RoleSessionName = "Pod"
            };

            using (var client = new AmazonSecurityTokenServiceClient(Region))
            {
                var response = await client.AssumeRoleAsync(request);
                Credentials = response.Credentials;
            }
        }
    }
}