using System;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.SecretsManager.Model;

namespace Olive.Aws.Providers
{
    class SecretsManager : AwsSecretProvider<Amazon.SecretsManager.AmazonSecretsManagerClient>
    {
        internal override string Download(string secretId)
        {
            var request = new GetSecretValueRequest { SecretId = secretId };
            var response = (GetSecretValueResponse)AwsClient.GetType().GetMethod("GetSecretValue", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(AwsClient, new object[] { request });
            return response.SecretString;
        }
    }
}