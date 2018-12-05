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

        public static void LoadAwsDevIdentity(this IConfiguration @this)
        {
            AWSConfigs.RegionEndpoint = RegionEndpoint.EUWest1;
            FallbackCredentialsFactory.Reset();

            var accessKey = @this["Aws:Credentials:AccessKey"];
            var secret = @this["Aws:Credentials:Secret"];
            FallbackCredentialsFactory.CredentialsGenerators.Insert(0, () => new BasicAWSCredentials(accessKey, secret));
        }

        public static void LoadAwsSecrets(this IConfiguration @this) => new Secrets(@this).Load();

        public static void LoadAwsIdentity(this IConfiguration @this,
            Action<IDictionary<string, string>> onLoaded)
        {
            Secrets.Loaded.Handle(onLoaded);
            @this.LoadAwsIdentity();
        }
    }
}