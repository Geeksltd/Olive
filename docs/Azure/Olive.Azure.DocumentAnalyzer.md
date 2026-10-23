**Olive.Azure.DocumentAnalyzer**

## Overview
The `Olive.Azure.DocumentAnalyzer` provides methods to interact with Azure Document Intelligence API to analyze documents, list models, and build document analyzers.

## Configuration
Ensure that the following configuration values are set before using the API:
- `Azure:DocumentIntelligence:Endpoint` - The endpoint URL for the Azure Document Intelligence service.
- `Azure:DocumentIntelligence:ApiKey` - The API key required for authentication.
- `Azure:DocumentIntelligence:ApiVersion` - The version of the API being used.

## Methods

### `BuildDocumentAnalyzer`
**Description:**
Creates and trains a document model using files stored in an Azure Blob Storage container.

**Signature:**
```csharp
public async Task<BuildDocumentAnalyzerResponse> BuildDocumentAnalyzer(
    string modelId,
    string trainingFolderName,
    string description = null,
    string storageContainerName = null)
```

**Parameters:**
- `modelId` (string) - The identifier for the model.
- `trainingFolderName` (string) - The folder containing training documents.
- `description` (string, optional) - A description of the model.
- `storageContainerName` (string, optional) - The name of the Azure Storage container.

**Returns:**
- `BuildDocumentAnalyzerResponse` - Contains details about the document analysis operation.

**Usage:**
```csharp
var result = await documentAnalyze.BuildDocumentAnalyzer("model123", "training-folder");
```

---

### `ListDocumentAnalyzeModelsAsync`
**Description:**
Retrieves a list of available document models from Azure Document Intelligence.

**Signature:**
```csharp
public async Task<DocumentAnalyzeModelResponse> ListDocumentAnalyzeModelsAsync(
    bool skipPrebuilt = true,
    bool useFormRecognizer = false)
```

**Parameters:**
- `skipPrebuilt` (bool) - If true, excludes prebuilt models.
- `useFormRecognizer` (bool) - If true, uses Form Recognizer instead of Document Intelligence.

**Returns:**
- `DocumentAnalyzeModelResponse` - A list of available models.

**Usage:**
```csharp
var models = await documentAnalyze.ListDocumentAnalyzeModelsAsync();
```

---

### `AnalyzeDocumentAsync`
**Description:**
Analyzes a document using a specified model.

**Signature:**
```csharp
public async Task<string> AnalyzeDocumentAsync(
    byte[] fileData,
    string modelId,
    bool useFormRecognizer = false)
```

**Parameters:**
- `fileData` (byte[]) - The document data in byte format.
- `modelId` (string) - The ID of the document model to use.
- `useFormRecognizer` (bool) - If true, uses Form Recognizer instead of Document Intelligence.

**Returns:**
- `string` - The operation location URL to check the analysis results.

**Usage:**
```csharp
var resultUrl = await documentAnalyze.AnalyzeDocumentAsync(fileBytes, "model123");
```

---

### `AnalyzeDocumentAsyncWithPoll`
**Description:**
Analyzes a document and polls for the results until completion.

**Signature:**
```csharp
public async Task<DocumentAnalyzeResult> AnalyzeDocumentAsyncWithPoll(
    byte[] fileData,
    string modelId,
    bool useFormRecognizer = false)
```

**Returns:**
- `DocumentAnalyzeResult` - Contains extracted data from the document.

**Usage:**
```csharp
var analysisResult = await documentAnalyze.AnalyzeDocumentAsyncWithPoll(fileBytes, "model123");
```

---

### `GetPollResults`
**Description:**
Fetches the results of a previously started document analysis operation.

**Signature:**
```csharp
public async Task<DocumentAnalyzeResult> GetPollResults(string resultUrl)
```

**Returns:**
- `DocumentAnalyzeResult` - The final document analysis results.

**Usage:**
```csharp
var result = await documentAnalyze.GetPollResults(operationUrl);
```

--- 

## Error Handling
- If an API request fails, an exception will be thrown with the corresponding HTTP status code and error message.
- Network failures or incorrect API keys may cause authentication errors.
- Ensure that the provided model ID and training data exist in the configured Azure Blob Storage. 

## Notes
- The class interacts with Azure's Document Intelligence API, requiring a valid API key.
- Polling methods wait until the document processing completes before returning results.
- Ensure that the `endpoint` and `apiKey` are correctly configured before making API calls.

---
This documentation provides a structured reference for developers integrating with the `Olive.Azure.DocumentAnalyzer`. If further details are needed, refer to the official Azure Document Intelligence API documentation.

