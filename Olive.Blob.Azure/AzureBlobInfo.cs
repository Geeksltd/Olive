using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.BlobAzure
{
    public class AzureBlobInfo
    {
        internal static string StorageConnectionString => Config.Get<string>("AzureStorage:ConnectionString");
        internal static string StorageContainer => Config.Get<string>("AzureStorage:Container");

    }
}
