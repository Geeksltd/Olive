using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Olive.Aws.Providers;
using System;
using System.Collections.Generic;

namespace Olive.Aws
{
    public class Secrets : Dictionary<string, string>
    {
        public static event AwaitableEventHandler<Secrets> Loaded;
        IConfiguration Config;
        SecretProviderType SecretProviderType;
        string SecretId => Config["Aws:Secrets:Id"];
        string SecretString;

        internal Secrets(IConfiguration config, SecretProviderType providerType)
        {
            Config = config;
            SecretProviderType = providerType;
        }

        static ILogger Log => Olive.Log.For(typeof(RuntimeIdentity));

        internal void Load()
        {
            Download();

            var jsonProvider = new JsonConfigurationProvider(SecretString);
            jsonProvider.Load();

            foreach (var item in jsonProvider.GetData())
                Config[item.Key] = this[item.Key] = item.Value;

            Loaded.Raise(this);
        }

        void Download()
        {
            Log.Debug("Downloading secret: " + SecretId + "...");

            try
            {
                var secrets = AwsSecretProvider.GetProvider(SecretProviderType).Download(SecretId).RiskDeadlockAndAwaitResult();
                if (secrets.IsEmpty())
                    throw new Exception("AWS SecretString was empty!");

                SecretString = secrets;

                Log.Debug("Downloaded secrets successfully.");
            }
            catch (AggregateException ex)
            {
                Log.Error(ex.InnerException, "Failed to obtain the AWS secret: " + SecretId);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to obtain the AWS secret: " + SecretId);
                throw;
            }
        }
    }
}
