using System.Threading.Tasks;
using Amazon.SecretsManager.Model;

namespace Olive.Aws.Providers
{
    class SecretsManager : AwsSecretProvider<Amazon.SecretsManager.AmazonSecretsManagerClient>
    {
        internal override async Task<string> Download(string secretId)
        {
            var request = new GetSecretValueRequest { SecretId = secretId };
            var response = await AwsClient.GetSecretValueAsync(request).ConfigureAwait(false);
            return response.SecretString;
        }
    }
}