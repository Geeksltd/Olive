namespace Olive;

using System.Collections.Generic;

public class DocumentAnalyzeResult
{
    public string Status { get; set; }
    public AnalyzeResult? AnalyzeResult { get; set; }
}

public class AnalyzeResult
{
    public List<Document>? Documents { get; set; }
    public List<Page>? Pages { get; set; }
}

public class Page
{
    public int? PageNumber { get; set; }
    public decimal? Angle { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public List<Words>? Words { get; set; }
}

public class Words
{
    public string? Content { get; set; }
}

public class Document
{
    public Dictionary<string, Fields>? Fields { get; set; }
}

public class Fields
{
    public string? Type { get; set; }
    public string? ValueString { get; set; }
    public string? Content { get; set; }
    public ValueCurrency? ValueCurrency { get; set; }
    public List<BoundingRegions>? BoundingRegions { get; set; }
    public decimal? Confidence { get; set; }
    public List<ValueArray>? ValueArray { get; set; }
    public Dictionary<string, Fields>? ValueObject { get; set; }

}
public class BoundingRegions
{
    public int? PageNumber { get; set; }
    public List<float>? Polygon { get; set; }
}
public class ValueArray
{
    public string? Type { get; set; }
    public Dictionary<string, Fields>? ValueObject { get; set; }
}
public class ValueCurrency
{
    public string? CurrencySymbol { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? Amount { get; set; }
}