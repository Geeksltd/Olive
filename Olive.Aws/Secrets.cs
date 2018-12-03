using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Olive.Aws
{
    public class Secrets : Dictionary<string, string>
    {
        public readonly static AsyncEvent<Secrets> Loaded = new AsyncEvent<Secrets>();
        IConfiguration Config;
        string SecretId => Config["Aws:Secrets:Id"];
        string SecretString;

        internal Secrets(IConfiguration config)
        {
            Config = config;
        }

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
            var request = new GetSecretValueRequest { SecretId = SecretId };

            try
            {
                using (var client = new AmazonSecretsManagerClient())
                {
                    var response = client.GetSecretValueAsync(request).RiskDeadlockAndAwaitResult();
                    if (response.SecretString.IsEmpty())
                        throw new Exception("AWS SecretString was empty!");

                    SecretString = response.SecretString;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to obtain the AWS secret: " + request.SecretId, ex);
            }
        }
    }
}
