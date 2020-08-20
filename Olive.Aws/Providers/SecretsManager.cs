using Amazon.SecretsManager.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Aws.Providers
{
    class SecretsManager : AwsSecretProvider<Amazon.SecretsManager.AmazonSecretsManagerClient>
    {
        internal override async Task<string> Download(string secretId)
        {
            var request = new GetSecretValueRequest { SecretId = secretId };
            var response = await AwsClient.GetSecretValueAsync(request);
            return response.SecretString;
        }
    }
}
