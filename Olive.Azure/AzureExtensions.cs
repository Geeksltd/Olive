using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Olive.Azure;

namespace Olive
{
    public static class AzureExtensions
    {
        //public static void LoadAzureDevIdentity(this IConfiguration @this, bool loadSecrets = false)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Loads secret information from Azure App Configuration. 
        /// Required settings : Azure:Secrets:Key, Azure:Secrets:Endpoint
        /// </summary>
        /// <param name="this"></param>
        public static void LoadAzureSecrets(this IConfiguration @this) => new Secrets(@this).Load();
    }


}