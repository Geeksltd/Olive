namespace Olive;

using System;
using System.Collections.Generic;

public class BuildDocumentAnalyzerResponse
{
    public string OperationId { get; set; }
    public string Kind { get; set; }
    public string Status { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime LastUpdatedDateTime { get; set; }
    public string ResourceLocation { get; set; }
    public int PercentCompleted { get; set; }
    public Result Result { get; set; }
}

public class Result
{
    public Dictionary<string, DocType> DocTypes { get; set; }
    public string ModelId { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime ExpirationDateTime { get; set; }
    public string ApiVersion { get; set; }
    public string Description { get; set; }
}

public class DocType
{
    public FieldSchema FieldSchema { get; set; }
    public string BuildMode { get; set; }
    public Dictionary<string, double> FieldConfidence { get; set; }
}

public class FieldSchema
{
    public Dictionary<string, FieldType> Fields { get; set; }
}

public class FieldType
{
    public string Type { get; set; }
    public FieldItems Items { get; set; }
    public Dictionary<string, FieldType> Properties { get; set; }
}

public class FieldItems
{
    public string Type { get; set; }
    public Dictionary<string, FieldType> Properties { get; set; }
}
