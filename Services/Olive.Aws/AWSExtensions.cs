using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using Olive.Aws;
using System;

namespace Olive
{
    public static class AWSExtensions
    {
        public static void LoadAwsIdentity(this IConfiguration @this)
        {
            RuntimeIdentity.Load().WaitAndThrow();

            var secrets = ObtainRuntimeIdentitiesSecret(@this);

            foreach (var item in new JsonConfigurationProvider(secrets).GetData())
                @this[item.Key] = item.Value;
        }

        static string ObtainRuntimeIdentitiesSecret(IConfiguration config)
        {
            var request = new GetSecretValueRequest { SecretId = config["Aws:Secrets:Id"] };

            try
            {
                using (var client = new AmazonSecretsManagerClient(
                    RuntimeIdentity.Credentials,
                    RuntimeIdentity.Region))
                {
                    var response = client.GetSecretValueAsync(request).RiskDeadlockAndAwaitResult();
                    if (response.SecretString.IsEmpty())
                        throw new Exception("AWS SecretString was empty!");

                    return response.SecretString;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to obtain the AWS secret: " + request.SecretId, ex);
            }
        }
    }
}