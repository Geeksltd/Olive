namespace Olive.BlobAzure
{
    using Azure.Storage.Blobs;
    using Azure.Storage.Sas;
    using Azure.Storage;
    using System;

    public class SASTokenGenerator
    {
        string  storageAccountName = AzureBlobInfo.StorageAccountName;
        string  storageContainer = AzureBlobInfo.StorageContainer;
        string storageAccountKey = AzureBlobInfo.StorageAccountKey;

        public  Uri GenerateContainerSasUri(string storageContainerName = null)
        {
            // Create a BlobServiceClient using the storage account name and key
            BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri($"https://{storageAccountName}.blob.core.windows.net"), new StorageSharedKeyCredential(storageAccountName, storageAccountKey));

            if (!string.IsNullOrWhiteSpace(storageContainerName))
            {
                storageContainer = storageContainerName;
            }

            // Get a reference to the container
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(storageContainer);

            //// Ensure the container exists
            //containerClient.CreateIfNotExists();

            // Create a SAS token for the container
            BlobSasBuilder sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = storageContainer,
               
                Resource = "c", // 'c' indicates the SAS is for a container  'b' for blob, 'c' for container
                //StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // Start a bit earlier to account for clock skew
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(8), // Adjust as needed
            };

            // Set permissions - adjust as necessary
            sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write | BlobSasPermissions.Create | BlobSasPermissions.Add | BlobSasPermissions.Delete | BlobSasPermissions.List);


            // Generate the SAS token
            string sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(storageAccountName, storageAccountKey)).ToString();

            // Combine the SAS token with the container URI
            Uri sasUri = new Uri($"{containerClient.Uri}?{sasToken}");

            return sasUri;
        }

        //TODO : Needs to refactor this and combine with above method
        // creawte sas token for blob
        //public static string GetBlobSasToken(string blobName, AzureAccountInfoDto azureAccount)
        //{
        //    // Create a BlobServiceClient object which will be used to create a container client
        //    var blobServiceClient = new BlobServiceClient(azureAccount.ConnectionString);

        //    // Create the container and return a container client object
        //    var containerClient = blobServiceClient.GetBlobContainerClient(azureAccount.ContainerName);

        //    // Get a reference to a blob
        //    var blobClient = containerClient.GetBlobClient(blobName);

        //    // Create a SAS token that's valid for one hour.
        //    var sasBuilder = new BlobSasBuilder
        //    {
        //        BlobContainerName = blobClient.BlobContainerName,
        //        BlobName = blobClient.Name,
        //        Resource = "b", // 'b' for blob, 'c' for container
        //        StartsOn = DateTimeOffset.UtcNow.AddMinutes(-15), // Start a bit earlier to account for clock skew
        //        ExpiresOn = DateTimeOffset.UtcNow.AddHours(8), // Adjust as needed
        //    };

        //    // Set permissions - adjust as necessary
        //    sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write | BlobSasPermissions.Create | BlobSasPermissions.Add | BlobSasPermissions.Delete | BlobSasPermissions.List);


        //    // Specify the service version
        //    sasBuilder.Version = "2022-11-02";

        //    // var sasUri = blobClient.GenerateSasUri(sasBuilder);

        //    // Use the key to get the SAS token.
        //    //var sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(Config.Get("Azure:Storage:AccountName"), Config.Get("Azure:Storage:AccountKey")));


        //    // Generate the SAS token
        //    string sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(
        //        blobServiceClient.AccountName,
        //        azureAccount.AccountKey
        //    )).ToString();

        //    // Construct the full URI including the SAS token
        //    UriBuilder fullUri = new UriBuilder(blobClient.Uri)
        //    {
        //        Query = sasToken
        //    };

        //    return fullUri.ToString();
        //}
    }
}
