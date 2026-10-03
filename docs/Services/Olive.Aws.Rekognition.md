# Olive.Aws.Rekognition

## Overview
The `TextDetector` class provides functionality to interact with AWS Rekognition for text detection in images. It supports text extraction from images stored in S3 as well as images provided as byte streams.

## Configuration
Ensure the following configurations are set in your application:
- **AWS:Rekognition:S3Bucket** - The default S3 bucket where images are stored.
- **AWS:Rekognition:Region** - The AWS region where Rekognition services are deployed.
- **Blob:S3:Bucket** - An alternative bucket for storing images.

## Installation
To integrate this package into your project, install the necessary dependencies:
```sh
Install-Package AWSSDK.Rekognition
```

## AWS Clients
```csharp
public static IAmazonRekognition Client
```
- **Client**: Provides access to AWS Rekognition operations.

### `Region(Amazon.RegionEndpoint region)`
```csharp
public static void Region(Amazon.RegionEndpoint region)
```
- **Summary**: Creates and uses a new AWS Rekognition client for the specified region.
- **Usage**:
```csharp
TextDetector.Region(RegionEndpoint.USEast1);
```

## Text Detection Operations

### `GetTextDetetionResult`
```csharp
public static async Task<List<TextDetection>> GetTextDetetionResult(string photo)
```
- **Summary**: Retrieves text detection results for an image stored in the configured S3 bucket.
- **Usage**:
```csharp
var results = await TextDetector.GetTextDetetionResult("image.jpg");
foreach (var text in results)
{
    Console.WriteLine(text.DetectedText);
}
```
- **Warning**: Throws an exception if `AWS:Rekognition:S3Bucket` is not configured.

### `GetTextDetetionResult`
```csharp
public static async Task<List<TextDetection>> GetTextDetetionResult(string photo, string bucketName)
```
- **Summary**: Retrieves text detection results for an image stored in a specific S3 bucket.
- **Usage**:
```csharp
var results = await TextDetector.GetTextDetetionResult("image.jpg", "custom-bucket");
```

### `GetTextDetetionResult`
```csharp
public static async Task<List<TextDetection>> GetTextDetetionResult(MemoryStream photo)
```
- **Summary**: Retrieves text detection results for an image provided as a byte stream.
- **Usage**:
```csharp
using (var memoryStream = new MemoryStream(File.ReadAllBytes("image.jpg")))
{
    var results = await TextDetector.GetTextDetetionResult(memoryStream);
}
```

## Error Handling
If an error occurs while processing the image, an exception is thrown. Ensure to wrap calls in a try-catch block:
```csharp
try
{
    var results = await TextDetector.GetTextDetetionResult("image.jpg");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Full Example
```csharp
TextDetector.Region(RegionEndpoint.USEast1);
var results = await TextDetector.GetTextDetetionResult("image.jpg");
foreach (var text in results)
{
    Console.WriteLine(text.DetectedText);
}
```

## Conclusion
The `TextDetector` class provides an easy-to-use wrapper for AWS Rekognition text detection, enabling efficient extraction of text from images stored in S3 or processed as byte streams.