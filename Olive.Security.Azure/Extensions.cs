using Azure.Identity;
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Olive.Security.Azure
{
    public static class Extensions
    {
        static Uri blobStorageUri;

        internal static Uri BlobStorageUri
        {
            get
            {
                if (blobStorageUri == null)
                {
                    var uriValue = Config.Get("Azure:KeyVault:CookieAuthentication:BlobStorageUri", Environment.GetEnvironmentVariable("AZURE_KEY_VALUT_COOKIE_AUTHENTICATION_BLOB_STORAGE_URI"));
                    if (uriValue.IsEmpty()) throw new Exception("Azure Key Valut blob storage uri is not specified in [Azure:KeyVault:CookieAuthentication:BlobStorageUri].");

                    blobStorageUri = uriValue.AsUri();
                }

                return blobStorageUri;
            }
        }
        /// <summary>
        /// Required key : Azure:KeyVault:CookieAuthentication:BlobStorageUri
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="keyVaultUri">https://[#name#].vault.azure.net/keys/DATAPROTECTIONKEY/</param>
        public static void PersistKeysToAzureBlobStorage(this IDataProtectionBuilder builder) =>
            builder.PersistKeysToAzureBlobStorage(BlobStorageUri, new DefaultAzureCredential());
    }
}