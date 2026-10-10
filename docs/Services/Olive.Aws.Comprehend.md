# Olive.Aws.Comprehend

## Overview
The `ComprehendClassifier` class provides functionality to interact with AWS Comprehend and AWS S3 for text classification. It supports creating and managing classifiers, running classification jobs, and handling data storage in S3.

## Configuration
- **AWS:Comprehend:S3Bucket** - The default S3 bucket for storing classification data.
- **AWS:Comprehend:S3ClassificationJobsBucket** - The bucket for classification jobs.
- **AWS:Comprehend:Region** - The AWS region where Comprehend services are deployed.
- **AWS:Comprehend:IAMRoleArn** - IAM Role ARN required for Comprehend operations.

## AWS Clients
```csharp
public static IAmazonS3 S3Client
public static IAmazonComprehend Client
```
- **S3Client**: Provides access to AWS S3 operations.
- **Client**: Provides access to AWS Comprehend operations.

### `Region(Amazon.RegionEndpoint region)`
```csharp
public static void Region(Amazon.RegionEndpoint region)
```
- **Summary**: Creates and uses a new AWS client in the specified region.
- **Usage**:
```csharp
ComprehendClassifier.Region(RegionEndpoint.USEast1);
```

## Classifier Management

### `CreateAsync`
```csharp
public static async Task<string> CreateAsync(string name, string Version, LanguageCode languageCode = null, DocumentClassifierMode mode = null)
```
- **Summary**: Creates and starts the training of Comprehend classification module based on AWS:Comprehend:S3Bucket bucket.
- **Returns**: The Amazon arn of the module needed to check the status and use the module.If Bucket is not specified will use Blob:S3:Bucket defualt bucket of the application.

### `CreateAsync`
```csharp
public static async Task<string> CreateAsync(string name, string Version, string bucketName, LanguageCode languageCode= null, DocumentClassifierMode  mode = null)
```
- **Summary**: Creates and starts the training of Comprehend classification module based on the bucket specified.
- **Returns**: The Amazon arn of the module needed to check the status and use the module.

### `DescribeClasifier`
```csharp
public static async Task<DocumentClassifierProperties> DescribeClasifier(string classifier_arn)
```
- **Summary**: Retrieves full details of a classifier.
- **Returns**: `DocumentClassifierProperties` containing classifier details.

### `GetClasifierStatus`
```csharp
public static async Task<string> GetClasifierStatus(string classifier_arn)
```
- **Summary**: Returns the current status of a classifier.

### `DeleteClassifier`
```csharp
public static async Task DeleteClassifier(string classifier_arn)
```
- **Summary**: Deletes a document classifier.

## Classification Operations

### `ClassifyByEndpoint`
```csharp
public static async Task<DocumentClass> ClassifyByEndpoint(string endpoint_arn, string documentText)
```
- **Summary**: Classifies a document using a trained classifier endpoint. 
- **Note**: Classifying by endpoint is costly; use `StartClasifyingJob` when possible.

### `StartClasifyingJob`
```csharp
public static async Task<string> StartClasifyingJob(string classifier_arn, string documetnKey, string outputfolder, string inputfolder)
```
- **Summary**: Starts the Clasifying job based on the location in the s3 bucket configured AWS:Comprehend:S3ClassificationJobsBucket.
- **Returns**: Gives back the job id that you can use with GetClasifyingJobResults() to get the results of a job.

### `GetClasifyingJobResults`
```csharp
public static async Task<DocumentClassificationJobProperties> GetClasifyingJobResults(string jobId, string nextToken = null)
```
- **Summary**: Returns an object containing the status of the classification job and the URI of the output file.

## S3 File Management

### `CopyFile`
```csharp
public static async Task<string> CopyFile(string sourceKey, string Destinationkey)
```
- **Summary**: Copies the file from AWS:Rekognition:S3SourceBucket to AWS:Comprehend:S3Bucket bucket that is the main comprehend bucket.
- **Returns**: The Checksum SHA256 of the saved file for confirmation if needed.If Bucket is not specified will use Blob:S3:Bucket defualt bucket of the application.

### `CopyFile`
```csharp
public static async Task<string> CopyFile(string sourceKey, string Destinationkey, string sourceBucket, string destinationBucket)
```
- **Summary**: Copies the file from specified source bucket and destination bucket to copy a file.
- **Returns**: The Checksum SHA256 of the saved file for confirmation if needed.

## Usage Example
```csharp
var classifierArn = await ComprehendClassifier.CreateAsync("MyClassifier", "v1");
var status = await ComprehendClassifier.GetClasifierStatus(classifierArn);
await ComprehendClassifier.DeleteClassifier(classifierArn);
```

## Conclusion
The `ComprehendClassifier` class provides an easy-to-use wrapper for AWS Comprehend classification tasks, enabling efficient training, management, and execution of classification models.
