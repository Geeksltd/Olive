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
            new Secrets(@this).Load();
        }

        public static void LoadAwsIdentity(this IConfiguration @this,
            Action<IDictionary<string, string>> onLoaded)
        {
            Secrets.Loaded.Handle(onLoaded);
            @this.LoadAwsIdentity();
        }
    }
}