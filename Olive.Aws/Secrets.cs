using Microsoft.Extensions.Configuration;
using Olive.Aws.Providers;

namespace Olive.Aws
{
    public class Secrets : Cloud.Secrets
    {
        SecretProviderType SecretProviderType;
        protected override string SecretId => Config["Aws:Secrets:Id"];

        internal Secrets(IConfiguration config, SecretProviderType providerType) : base(config)
        {
            SecretProviderType = providerType;
        }

        protected override string DownloadSecrets() =>
            AwsSecretProvider.GetProvider(SecretProviderType).Download(SecretId).RiskDeadlockAndAwaitResult();
    }
}
