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
        static string RegionName, RoleArn;

        static IConfiguration Config;
        public static AWSCredentials Credentials { get; private set; }

        public static async Task Load(IConfiguration config)
        {
            Config = config;
            RoleArn = Environment.GetEnvironmentVariable(VARIABLE);
            Environment.SetEnvironmentVariable(VARIABLE, null);

            RegionName = Config.GetValue("Aws:Region",
                defaultValue: Environment.GetEnvironmentVariable("AWS_RUNTIME_ROLE_REGION"));

            await Renew();
            new Thread(KeepRenewing).Start();
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
                    Console.WriteLine("Failed to renew AWS credentials.");
                    Console.WriteLine(ex.ToFullMessage());
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