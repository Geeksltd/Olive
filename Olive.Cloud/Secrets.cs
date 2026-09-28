using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Olive.Cloud
{
    public abstract class Secrets : Dictionary<string, string>
    {
        public static event AwaitableEventHandler<Secrets> Loaded;
        protected IConfiguration Config;
        protected string SecretString;

        protected abstract string DownloadSecrets();

        protected abstract string SecretId { get; }

        protected Secrets(IConfiguration config) => Config = config;

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
                {
                    Console.WriteLine("SecretString was empty: " + SecretId);
                    throw new Exception("SecretString was empty!");
                }

                SecretString = secrets;
            }
            catch (AggregateException ex)
            {
                // Critical: without its secrets the application cannot start. The inner errors are
                // details of that one failure, so they do not raise an alert of their own.
                Log.Critical(ex, "Failed to obtain the secret with errors: " + SecretId);
                ex.InnerExceptions.Do(e => Log.Error(e, "Failed to obtain the secret with error: " + SecretId));
                throw;
            }
            catch (Exception ex)
            {
                Log.Critical(ex, "Failed to obtain the secret: " + SecretId);
                throw;
            }
        }
    }
}