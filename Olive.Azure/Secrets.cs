using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Configuration;
using System;
using azIdentity = Azure.Identity;

namespace Olive.Azure
{
    public class Secrets : Cloud.Secrets
    {
        protected override string SecretId => Olive.Config.GetOrThrow("Azure:Secrets:Key");
        protected Uri EndpointUri => Olive.Config.GetOrThrow("Azure:Secrets:Endpoint").AsUri();

        internal Secrets(IConfiguration config) : base(config)
        {

        }

        protected override string DownloadSecrets()
        {
            var client = new ConfigurationClient(EndpointUri, new azIdentity.DefaultAzureCredential());
            return client.GetConfigurationSetting(SecretId, SecretId).Value.Value;
        }
    }
}
