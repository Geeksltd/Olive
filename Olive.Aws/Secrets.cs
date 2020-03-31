using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Olive.Aws
{
    public class Secrets : Dictionary<string, string>
    {
        public static event AwaitableEventHandler<Secrets> Loaded;
        IConfiguration Config;
        string SecretId => Config["Aws:Secrets:Id"];
        string SecretString;

        internal Secrets(IConfiguration config) => Config = config;

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

                Log.Debug("Downloaded secrets successfully.");
            }
            catch (AggregateException ex)
            {
                Log.Error(ex.InnerException, "Failed to obtain the AWS secret: " + request.SecretId);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to obtain the AWS secret: " + request.SecretId);
                throw;
            }
        }
    }
}
