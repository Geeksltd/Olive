using Amazon;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using System;
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

        public static AWSCredentials Credentials { get; private set; }

        static string RegionName => Environment.GetEnvironmentVariable("AWS_RUNTIME_ROLE_REGION");

        public static RegionEndpoint Region => RegionEndpoint.GetBySystemName(RegionName);

        internal static async Task Load()
        {
            using (var client = new AmazonSecurityTokenServiceClient(Region))
            {
                var response = await client.AssumeRoleAsync(CreateRequest());
                Credentials = response.Credentials;
            }
        }

        static AssumeRoleRequest CreateRequest()
        {
            var roleArn = Environment.GetEnvironmentVariable(VARIABLE);
            Environment.SetEnvironmentVariable(VARIABLE, null);

            return new AssumeRoleRequest
            {
                RoleArn = roleArn,
                DurationSeconds = (int)12.Hours().TotalSeconds,
                ExternalId = "Pod",
                RoleSessionName = "Pod",
                Policy = "{\"Version\":\"2012-10-17\",\"Statement\":[{\"Sid\":\"Stmt1\",\"Effect\":\"Allow\",\"Action\":\"s3:*\",\"Resource\":\"*\"}]}"
            };
        }
    }
}