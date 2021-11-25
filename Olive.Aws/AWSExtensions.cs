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
        [Obsolete("This is intended for use in Production under Kubernetes pods as a workaround for adding AWS Roles to specific pods as opposed to the host server." +
            "This is not needed in Lambda as you can allocate a role to lambda directly.")]
        public static void LoadAwsIdentity(this IConfiguration @this)
        {
            RuntimeIdentity.Load(@this).WaitAndThrow();
            @this.LoadAwsSecrets();
        }

        /// <summary>
        /// Use this if you want to have AWS calls made under the current machine's Role, 
        /// or the user specified in appSettings under (Aws { Credentials { AccessKey: ... , Secret: ... } }).
        /// </summary>
        public static void LoadAwsDevIdentity(this IConfiguration @this, bool loadSecrets = false)
        {
            var accessKey = @this["Aws:Credentials:AccessKey"];
            var secret = @this["Aws:Credentials:Secret"];
            var endpoint = RegionEndpoint.GetBySystemName(@this["Aws:Region"].Or(RegionEndpoint.EUWest1.SystemName));

            @this.LoadAwsDevIdentity(accessKey, secret, endpoint, loadSecrets);
        }

        [Obsolete("Instead of hardcoding accessKey and secret, either use the default host credentials (role or default user) " +
            "or at least use appSettings (under Aws { Credentials { AccessKey: ... , Secret: ... } }).")]
        /// <summary>
        /// Use this if you want to have AWS calls made under a user.
        /// </summary>
        public static void LoadAwsDevIdentity(this IConfiguration @this, string accessKey, string secret, RegionEndpoint endpoint, bool loadSecrets)
        {
            AWSConfigs.RegionEndpoint = endpoint;

            if (accessKey.HasValue() && secret.HasValue())
            {
                FallbackCredentialsFactory.Reset();
                FallbackCredentialsFactory.CredentialsGenerators.Insert(0, () => new BasicAWSCredentials(accessKey, secret));
            }
            else
            {
                Log.For(typeof(AWSExtensions))
                    .Warning("Using the machine default AWS Role since there is no secret in appSettings under" +
                     "Aws { Credentials { AccessKey: ... , Secret: ... } }");
            }

            if (loadSecrets) @this.LoadAwsSecrets();
        }

        public static void LoadAwsSecrets(this IConfiguration @this, SecretProviderType provider = SecretProviderType.SecretsManager) => new Secrets(@this, provider).Load();

        public static void LoadAwsIdentity(this IConfiguration @this, Action<IDictionary<string, string>> onLoaded)
        {
            Secrets.Loaded += x => onLoaded(x.Args);
            @this.LoadAwsIdentity();
        }
    }
}