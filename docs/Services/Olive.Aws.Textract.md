# Olive.Aws.Textract

## Overview
The `Olive.Aws.Textract` provides functionality for extracting text from documents using AWS Textract. It supports text extraction from S3 buckets and in-memory documents, as well as asynchronous processing for large documents.

## Configuration
Ensure the following configurations are set in your application:
- **AWS:Rekognition:S3Bucket** - The bucket where documents are stored.
- **AWS:Rekognition:S3OutputBucket** - The bucket where output data is stored. 

## AWS Clients
```csharp
public static IAmazonTextract Client
```
- **Client**: Provides access to AWS Textract operations.

### `Region(Amazon.RegionEndpoint region)`
```csharp
public static void Region(Amazon.RegionEndpoint region)
```
- **Summary**: Creates and uses a new AWS Textract client for the specified region.
- **Usage**:
```csharp
AmazonTextract.Region(RegionEndpoint.USEast1);
```

## Text Extraction Operations

### `ExtractTextString`
```csharp
public static async Task<string> ExtractTextString(string documentKey)
```
- **Summary**: Extracts text as a single string from a document stored in S3.
- **Usage**:
```csharp
var text = await AmazonTextract.ExtractTextString("document.pdf");
```

### `ExtractTextBlocks`
```csharp
public static async Task<List<Block>> ExtractTextBlocks(string documentKey)
```
- **Summary**: Extracts text as structured blocks from a document stored in S3.
- **Usage**:
```csharp
var blocks = await AmazonTextract.ExtractTextBlocks("document.pdf");
```

### `StartExtraction`
```csharp
public static async Task<string> StartExtraction(string documentKey, string prefix)
```
- **Summary**: Starts an asynchronous text extraction job for a document in S3.
- **Usage**:
```csharp
var jobId = await AmazonTextract.StartExtraction("document.pdf", "prefix");
```

### `GetJobResultBlocks`
```csharp
public static async Task<TextDetectionBlockResults> GetJobResultBlocks(string jobId, string nextToken = null)
```
- **Summary**: Retrieves block results from an asynchronous text extraction job.
- **Usage**:
```csharp
var results = await AmazonTextract.GetJobResultBlocks(jobId);
```

### `GetJobResultText`
```csharp
public static async Task<TextDetectionTextResults> GetJobResultText(string jobId, string nextToken = null)
```
- **Summary**: Retrieves extracted text results from an asynchronous text extraction job.
- **Usage**:
```csharp
var textResults = await AmazonTextract.GetJobResultText(jobId);
```

### `StartAnalyzeDocument`
```csharp
public static async Task<string> StartAnalyzeDocument(string documentKey, string prefix)
```
- **Summary**: Starts an analysis job to extract structured data (e.g., forms, tables) from a document in S3.
- **Usage**:
```csharp
var jobId = await AmazonTextract.StartAnalyzeDocument("document.pdf", "prefix");
```

### `GetAnalyzeJobResultBlocks`
```csharp
public static async Task<TextDetectionBlockResults> GetAnalyzeJobResultBlocks(string jobId, string nextToken = null)
```
- **Summary**: Retrieves structured block results from an asynchronous document analysis job.
- **Usage**:
```csharp
var analysisBlocks = await AmazonTextract.GetAnalyzeJobResultBlocks(jobId);
```

### `GetAnalyzeJobResultText`
```csharp
public static async Task<TextDetectionTextResults> GetAnalyzeJobResultText(string jobId, string nextToken = null)
```
- **Summary**: Retrieves extracted text from an asynchronous document analysis job.
- **Usage**:
```csharp
var analysisText = await AmazonTextract.GetAnalyzeJobResultText(jobId);
```

## Full Example
```csharp
AmazonTextract.Region(RegionEndpoint.USEast1);
var text = await AmazonTextract.ExtractTextString("document.pdf");
Console.WriteLine(text);
```

## Conclusion
The `Olive.Aws.Textract` provides a simple and effective way to extract text from documents using AWS Textract. It supports synchronous and asynchronous processing for efficient text retrieval.