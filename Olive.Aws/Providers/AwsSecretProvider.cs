using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Aws.Providers
{
    abstract class AwsSecretProvider
    {
        internal static AwsSecretProvider GetProvider(SecretProviderType type)
        {
            switch (type)
            {
                case SecretProviderType.SystemsManagerParameter:
                    return new SystemsManagerParameter();
                default:
                    return new SecretsManager();

            }
        }

        internal abstract Task<string> Download(string secretId);
    }
}
