using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.BlobAzure
{
    public class AzureBlobInfo
    {
        internal static string StorageConnectionString => Config.Get<string>("AzureStorage:ConnectionString");
        internal static string StorageContainer => Config.Get<string>("AzureStorage:Container");
        internal static string StorageAccountName => Config.Get<string>("AzureStorage:AccountName");
        public static string StorageAccountKey => Config.Get<string>("AzureStorage:AccountKey");
    }
}