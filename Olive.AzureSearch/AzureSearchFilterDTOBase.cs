namespace Olive.AzureSearch
{
    using Azure.Search.Documents.Indexes;
    using Azure.Search.Documents.Indexes.Models;

    public abstract class AzureSearchFilterDTOBase
    {
        [SimpleField(IsKey = true)]
        public string Id { get; set; }
    }
}
