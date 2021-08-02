using System.Threading.Tasks;

namespace Olive.Aws.Providers
{
    abstract class AwsSecretProvider
    {
        internal static AwsSecretProvider GetProvider(SecretProviderType type)
        {
            return type switch
            {
                SecretProviderType.SystemsManagerParameter => new SystemsManagerParameter(),
                _ => new SecretsManager(),
            };
        }

        internal abstract Task<string> Download(string secretId);
    }
}
