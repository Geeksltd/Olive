using Amazon;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using Olive.Aws;
using System;
using System.Collections.Generic;

namespace Olive
{
    public static class AWSExtensions
    {
        public static void LoadAwsIdentity(this IConfiguration @this)
        {
            RuntimeIdentity.Load(@this).WaitAndThrow();
            @this.LoadAwsSecrets();
        }

        public static void LoadAwsDevIdentity(this IConfiguration @this, bool loadSecrets = false)
        {
            var accessKey = @this["Aws:Credentials:AccessKey"];
            var secret = @this["Aws:Credentials:Secret"];
            var endpoint = RegionEndpoint.GetBySystemName(@this["Aws:Region"].Or(RegionEndpoint.EUWest1.SystemName));

            @this.LoadAwsDevIdentity(accessKey, secret, endpoint, loadSecrets);
        }

        /// <summary>
        /// Use this if you want to temporarily simulate production environment access for debugging.
        /// The accessKey and secret are usually that of a root admin user.
        /// </summary>
        public static void LoadAwsDevIdentity(this IConfiguration @this, string accessKey, string secret, RegionEndpoint endpoint, bool loadSecrets)
        {
            AWSConfigs.RegionEndpoint = endpoint;
            FallbackCredentialsFactory.Reset();
            FallbackCredentialsFactory.CredentialsGenerators.Insert(0, () => new BasicAWSCredentials(accessKey, secret));
            if (loadSecrets)
                @this.LoadAwsSecrets();
        }

        public static void LoadAwsSecrets(this IConfiguration @this, SecretProviderType provider = SecretProviderType.SecretsManager) => new Secrets(@this, provider).Load();

        public static void LoadAwsIdentity(this IConfiguration @this, Action<IDictionary<string, string>> onLoaded)
        {
            Secrets.Loaded += x => onLoaded(x.Args);
            @this.LoadAwsIdentity();
        }
    }
}