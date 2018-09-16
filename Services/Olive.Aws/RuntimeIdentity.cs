using Amazon;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Microsoft.Extensions.Configuration;
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

            RegionName = Context.Current.Config.GetValue("Aws:Region",
                defaultValue: Environment.GetEnvironmentVariable("AWS_RUNTIME_ROLE_REGION"));
        }

        public static RegionEndpoint Region
        {
            get
            {
                if (RegionName.IsEmpty())
                    throw new Exception("Region name is not specified in either Environment Variable of 'AWS_RUNTIME_ROLE_REGION' or config value of 'Aws:Region'.");

                return RegionEndpoint.GetBySystemName(RegionName);
            }
        }

        public static async Task Load()
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