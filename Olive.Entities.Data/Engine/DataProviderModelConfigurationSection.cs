using System.Collections.Generic;

namespace Olive.Entities.Data
{
    public class DataProviderModelConfigurationSection
    {
        public List<DataProviderFactoryInfo> Providers { get; set; }

        /// <summary>
        /// Gets or sets the SyncFilePath of this DataProviderModelConfigurationSection.
        /// </summary>
        public string SyncFilePath { get; set; }

        /// <summary>
        /// Gets or sets the SyncFilePath of this DataProviderModelConfigurationSection.
        /// </summary>
        public string FileDependancyPath { get; set; }
    }
}