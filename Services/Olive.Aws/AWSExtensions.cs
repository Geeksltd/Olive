using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.DependencyInjection;
using Olive.Aws;
using System;
using System.Threading.Tasks;

namespace Olive
{
    public static class AWSExtensions
    {
        public static IServiceCollection AddAwsIdentity(this IServiceCollection @this)
        {
            Task.Factory.RunSync(LoadIdentityAndSecrets);
            return @this;
        }

        static async Task LoadIdentityAndSecrets()
        {
            await RuntimeIdentity.Load();

            var secrets = await ObtainRuntimeIdentitiesSecret();

            Config.AddConfiguration(x => x.Add(new JsonConfigurationSource(secrets)));
        }

        static async Task<string> ObtainRuntimeIdentitiesSecret()
        {
            var request = new GetSecretValueRequest { SecretId = Config.Get("Aws:Secrets:Id") };

            try
            {
                using (var client = new AmazonSecretsManagerClient(RuntimeIdentity.Credentials,
                    RuntimeIdentity.Region))
                {
                    var response = await client.GetSecretValueAsync(request);
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