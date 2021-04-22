using Microsoft.AspNetCore.DataProtection;
using System;
using System.Linq;
using azBlobs = Azure.Storage.Blobs;
using azIdentity = Azure.Identity;

namespace Olive.Security.Azure
{
    public static class Extensions
    {

        internal static string BlobStorageContainerUri
        {
            get
            {
                var uriValue = Config.Get("Azure:KeyVault:CookieAuthentication:BlobStorageUri", Environment.GetEnvironmentVariable("AZURE_KEY_VALUT_COOKIE_AUTHENTICATION_BLOB_STORAGE_URI"));
                if (uriValue.IsEmpty()) throw new Exception("Azure Key Valut blob storage uri is not specified.");
                return uriValue;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="keyVaultUri">https://[#name#].vault.azure.net/keys/DATAPROTECTIONKEY/</param>
        public static void PersistKeysToAzureBlobStorage(this IDataProtectionBuilder builder)
        {
            var containerName = BlobStorageContainerUri.Split("/").Last();
            var blobUri = BlobStorageContainerUri.TrimEnd("/" + containerName);
            var blobServiceClient = new azBlobs.BlobServiceClient(blobUri.AsUri(), new azIdentity.DefaultAzureCredential());
            var container = blobServiceClient.GetBlobContainerClient(containerName);
            var blobName = BlobStorageContainerUri.TrimStart("https://").Split(".").First();

            container.CreateIfNotExists();
            builder
                .PersistKeysToAzureBlobStorage(container.GetBlobClient(blobName));
        }
    }
}
