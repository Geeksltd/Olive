**# Olive.Azure.DocumentClassification**

## Overview
The `Olive.Azure.DocumentClassification` provides methods to interact with the Azure Document Intelligence service. It enables listing available classifiers and classifying documents either from a local file or a remote URL.
 
---

## Configuration
Ensure the following configuration settings are available in your application settings:
```json
{
  "Azure": {
    "Classification": {
      "Endpoint": "<Your Azure Endpoint>",
      "ApiKey": "<Your API Key>",
      "ModelId": "<Your Default Model ID>",
      "ApiVersion": "<API Version>"
    }
  }
}
```

---

## Methods

### `Task<DocumentClassifierResponse> ListDocumentClassifiersAsync(bool useFormRecognizer = false)`
#### Description
Retrieves a list of available document classifiers from Azure Document Intelligence.

#### Parameters
- `useFormRecognizer` *(bool, optional)* – If `true`, uses the Form Recognizer API instead of Document Intelligence.

#### Returns
- `DocumentClassifierResponse`: A list of available document classifiers.

#### Usage Example
```csharp
var classifier = new DocumentClassifier();
var classifiers = await classifier.ListDocumentClassifiersAsync();
```

---

### `Task<DocumentClassifierResult> ClassifyDocumentAsync(byte[] fileData, string modelId)`
#### Description
Classifies a document from a given file data using a specified model ID.

#### Parameters
- `fileData` *(byte[])* – The document file in byte array format.
- `modelId` *(string)* – The classifier model ID to use.

#### Returns
- `DocumentClassifierResult`: The classification result containing document type and confidence score.

#### Usage Example
```csharp
byte[] fileBytes = File.ReadAllBytes("document.pdf");
var classifier = new DocumentClassifier();
var result = await classifier.ClassifyDocumentAsync(fileBytes, "myModelId");
```

---

### `Task<DocumentClassifierResult> ClassifyDocumentAsync(string filePathOrURI, string modelId)`
#### Description
Classifies a document from a given file path or remote URL.

#### Parameters
- `filePathOrURI` *(string)* – The path to the document file or a URL.
- `modelId` *(string)* – The classifier model ID to use.

#### Returns
- `DocumentClassifierResult`: The classification result.

#### Usage Example
```csharp
var classifier = new DocumentClassifier();
var result = await classifier.ClassifyDocumentAsync("https://example.com/document.pdf", "myModelId");
```

---

## Supporting Classes

### `DocumentClassifierResponse`
#### Properties
- `List<Value> Value` – The list of classifier models available.

### `DocumentClassifierResult`
#### Properties
- `string DocumentType` – The classified document type.
- `decimal Confidence` – The confidence score of the classification.

---

## Notes
- This class uses Azure Document Intelligence API to classify documents.
- Ensure valid API keys and endpoints are configured before using the service.
- If an invalid file path or URL is provided, an exception will be thrown.

