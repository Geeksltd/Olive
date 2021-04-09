using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Olive.Aws.Providers;
using System;
using System.Collections.Generic;

namespace Olive.Aws
{
    public class Secrets : Olive.Cloud.Secrets
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
