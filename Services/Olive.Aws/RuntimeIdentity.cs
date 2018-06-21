using System;
using System.IO;
using System.Text;

using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime;

namespace Olive.Aws
{
    /// <summary>
    /// Specifies the current application's runtime IAM identity.
    /// All AWS services, e.g. S3, etc, should use this.
    /// </summary>
    public class RuntimeIdentity
    {
        public static string AccessKey { get; private set; }

        public static string SecretKey { get; private set; }

        public static BasicAWSCredentials Credentials => new BasicAWSCredentials(AccessKey, SecretKey);

        internal static void Load()
        {
            AccessKey = Config.GetOrThrow("Aws:Identity:Runtime:AccessKey");

            // Load the secret using startup identity


        }

        static async Task<string> ObtainRuntimeIdentitiesSecret()
        {
            var startUpAccessKey = "Aws:Identity:Startup:AccessKey";
            var startUpSecret = "Aws:Identity:Startup:SecretKey";

            var region = RegionEndpoint.GetBySystemName("eu-west-1");
            var startUpCredentials = new BasicAWSCredentials(startUpAccessKey, startUpSecret);
            var secretName = "Runtime-Identities-Secrets";

            try
            {
                using (var client = new AmazonSecretsManagerClient(startUpCredentials, region))
                {
                    var response = await client.GetSecretValueAsync(new GetSecretValueRequest { SecretId = secretName });
                    if (response.SecretString.IsEmpty()) throw new Exception("AWS SecretString was empty!");
                    return response.SecretString;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to obtain the Runtime-Identities-Secrets", ex);
            }
        }
    }
}