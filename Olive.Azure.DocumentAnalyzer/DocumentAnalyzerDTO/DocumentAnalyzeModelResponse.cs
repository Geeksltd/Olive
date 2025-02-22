namespace Olive;

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DocumentAnalyzeModelResponse
{
    public List<Value> Value { get; set; }
}

public class Value
{
    public string ModelId { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public string ApiVersion { get; set; }
    public string Description { get; set; }
    public DateTime? ExpirationDateTime { get; set; } // Nullable for models that don't have an expiration date
}  
