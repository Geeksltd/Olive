using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Configuration;

namespace Olive.Azure
{
    public class Secrets : Cloud.Secrets
    {
        protected override string SecretId => Olive.Config.GetOrThrow("Azure:Secrets:Key");
        protected string ConnectionString => Olive.Config.GetOrThrow("Azure:Secrets:ConnectionString");

        internal Secrets(IConfiguration config) : base(config)
        {

        }

        protected override string DownloadSecrets()
        {
            var client = new ConfigurationClient(ConnectionString);
            return client.GetConfigurationSetting(SecretId, SecretId).Value.Value;
        }
    }
}
