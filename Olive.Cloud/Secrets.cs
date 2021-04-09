using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Olive.Cloud
{
    public abstract class Secrets : Dictionary<string, string>
    {
        public static event AwaitableEventHandler<Secrets> Loaded;
        protected IConfiguration Config;
        protected string SecretString;

        protected abstract string DownloadSecrets();
        protected abstract string SecretId { get; }

        protected Secrets(IConfiguration config)
        {
            Config = config;
        }

        static ILogger Log => Olive.Log.For(typeof(Secrets));

        public void Load()
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
            try
            {
                var secrets = DownloadSecrets();
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
                Log.Error(ex, "Failed to obtain the secret: " + SecretId);
                throw;
            }
        }


    }
}
