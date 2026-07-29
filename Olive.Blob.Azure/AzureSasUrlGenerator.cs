using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Olive;
using Olive.Entities;
using System;

namespace Olive.BlobAzure
{
    public class AzureSasUrlGenerator : IAzureSasUrlGenerator
    {
        public TimeSpan DefaultTimeout { get; }

        public AzureSasUrlGenerator() : this(TimeSpan.FromMinutes(30)) { }

        public AzureSasUrlGenerator(TimeSpan defaultTimeout) => DefaultTimeout = defaultTimeout;

        public string Sign(Blob blob, TimeSpan? timeout = null) =>
            Sign(blob.GetKey(), timeout, blob.GetMimeType(), contentDisposition: "inline");

        public string Sign(string key, TimeSpan? timeout = null) =>
            Sign(key, timeout, contentType: null, contentDisposition: null);

        string Sign(string key, TimeSpan? timeout, string contentType, string contentDisposition)
        {
            var accountName = AzureBlobInfo.StorageAccountName;
            var accountKey = AzureBlobInfo.StorageAccountKey;
            var container = AzureBlobInfo.StorageContainer;

            if (accountName.IsEmpty() || accountKey.IsEmpty())
            {
                var connectionString = AzureBlobInfo.StorageConnectionString;
                if (connectionString.HasValue())
                {
                    var client = new BlobServiceClient(connectionString);
                    var blobClient = client.GetBlobContainerClient(container).GetBlobClient(key);
                    if (!blobClient.CanGenerateSasUri)
                        throw new InvalidOperationException("Azure blob client cannot generate SAS. Configure AzureStorage:AccountName and AzureStorage:AccountKey.");

                    var builder = CreateSasBuilder(container, key, timeout, contentType, contentDisposition);
                    return blobClient.GenerateSasUri(builder).ToString();
                }

                throw new InvalidOperationException("Azure Storage account credentials are not configured for SAS generation.");
            }

            var credential = new StorageSharedKeyCredential(accountName, accountKey);
            var containerClient = new BlobContainerClient(
                new Uri($"https://{accountName}.blob.core.windows.net/{container}"),
                credential);
            var blob = containerClient.GetBlobClient(key);
            return blob.GenerateSasUri(CreateSasBuilder(container, key, timeout, contentType, contentDisposition)).ToString();
        }

        BlobSasBuilder CreateSasBuilder(
            string container,
            string key,
            TimeSpan? timeout,
            string contentType,
            string contentDisposition)
        {
            var builder = new BlobSasBuilder
            {
                BlobContainerName = container,
                BlobName = key,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.Add(timeout ?? DefaultTimeout)
            };

            if (contentType.HasValue())
                builder.ContentType = contentType;

            if (contentDisposition.HasValue())
                builder.ContentDisposition = contentDisposition;

            builder.SetPermissions(BlobSasPermissions.Read);
            return builder;
        }
    }
}
