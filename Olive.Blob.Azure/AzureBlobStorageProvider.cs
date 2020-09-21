using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Olive.BlobAzure
{
    public class AzureBlobStorageProvider : IBlobStorageProvider
    {
        BlobServiceClient BlobServiceClient => new BlobServiceClient(AzureBlobInfo.StorageConnectionString);
        BlobContainerClient BlobContainerClient;

        static ILogger Log => Olive.Log.For(typeof(AzureBlobStorageProvider));

        public bool CostsToCheckExistence() => true;

        private async Task<BlobContainerClient> GetBlobContainer()
        {
            if (BlobContainerClient == null)
                return BlobContainerClient = BlobServiceClient.GetBlobContainerClient(AzureBlobInfo.StorageContainer);


            return BlobContainerClient;
        }

        public async Task SaveAsync(Blob document)
        {
            var containerClient = await GetBlobContainer();
            try
            {
                Log.Debug("Blob create upload object");
                var blobClient = containerClient.GetBlobClient(document.GetKey());

                Log.Debug("Upload Blob to Azure");
                using (var dataStream = new MemoryStream(await document.GetFileDataAsync()))
                    await blobClient.UploadAsync(dataStream, true);
            }
            catch (Exception ex)
            {
                Log.Debug("Save blob to azure ex: " + ex.Message);
                throw;
            }
        }

        public async Task<bool> FileExistsAsync(Blob document)
        {
            var blobContainer = await GetBlobContainer();
            var key = document.GetKey();
            var blobClient = blobContainer.GetBlobClient(key);

            try
            {
                return await blobClient.ExistsAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to check azure blob exist {key} key");
                throw;
            }
        }

        public async Task<byte[]> LoadAsync(Blob document)
        {
            var blobContainer = await GetBlobContainer();
            var key = document.GetKey();
            var blobClient = blobContainer.GetBlobClient(key);
            try
            {
                var file = await blobClient.DownloadAsync();
                return ToByte(file.Value.Content);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to load azure blob with {key} key");
                throw;
            }
        }

        public async Task DeleteAsync(Blob document)
        {
            if (document.IsEmpty())
                return;

            var blobContainer = await GetBlobContainer();
            var key = document.GetKey();

            try
            {
                await blobContainer.DeleteBlobAsync(document.GetKey());
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to delete document with {key} key on Azure");
                throw;
            }
        }

        public byte[] ToByte(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
