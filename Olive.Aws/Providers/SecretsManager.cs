using System;
using System.Threading.Tasks;
using Amazon.SecretsManager.Model;

namespace Olive.Aws.Providers
{
    class SecretsManager : AwsSecretProvider<Amazon.SecretsManager.AmazonSecretsManagerClient>
    {
        internal override Task<string> Download(string secretId)
        {
            var request = new GetSecretValueRequest { SecretId = secretId };
            return AwsClient.GetSecretValueAsync(request).Get(x => x.SecretString);
        }
    }
}