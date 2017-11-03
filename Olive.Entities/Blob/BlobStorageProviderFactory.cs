using System.Collections.Generic;

namespace Olive.Entities
{
    public class BlobStorageProviderFactory
    {
        public static IBlobStorageProvider DefaultProvider = new DiskBlobStorageProvider();

        /// <summary>
        /// This is to be configured in Global.asax if a different provider is needed for specific files.
        /// Example: Olive.Entities.BlobStorageProviderFactory.Add("Customer.Logo", new MySpecialStorageProvider);
        /// </summary>
        public static Dictionary<string, IBlobStorageProvider> Providers = new Dictionary<string, IBlobStorageProvider>();

        /// <summary>
        /// In the format: {type}.{property} e.g. Customer.Logo.
        /// </summary>
        internal static IBlobStorageProvider GetProvider(string folderName)
        {
            if (folderName.IsEmpty()) return DefaultProvider;

            return Providers.GetOrDefault(folderName) ?? DefaultProvider;
        }
    }
}
