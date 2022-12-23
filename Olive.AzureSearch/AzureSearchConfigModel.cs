namespace Olive.AzureSearch
{
    public class AzureSearchConfigModel
    {
        public string AdminKey { get; set; }
        public string ServiceUrl { get; set; }
        public bool Enabled { get; set; }
        public string DefaultIndex { get; set; }
    }
}
