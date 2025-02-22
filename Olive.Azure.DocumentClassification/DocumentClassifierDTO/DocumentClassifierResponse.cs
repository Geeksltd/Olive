namespace Olive;

public class DocumentClassifierResponse
{
    public List<Value> Value { get; set; }
}

public class Value
{
    public string ClassifierId { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime ExpirationDateTime { get; set; }
    public string ApiVersion { get; set; }
    public Dictionary<string, DocumentType> DocTypes { get; set; }
    public string Description { get; set; }
}

public class DocumentType
{
    public AzureBlobFileListSource AzureBlobFileListSource { get; set; }
}

public class AzureBlobFileListSource
{
    public string ContainerUrl { get; set; }
    public string FileList { get; set; }
}
