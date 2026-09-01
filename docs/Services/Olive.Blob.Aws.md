# Olive.Blob.Aws

This document outlines the public methods, variables, and properties in the `Olive.BlobAws` namespace, designed to help users interact with Amazon S3 for file storage, retrieval, and management. It assumes you are integrating this package into your project and need to use its public API.

--- 

## Classes and Public Members

### 1. `S3PresignedUrlGenerator`
A class to generate presigned URLs for S3 objects, implementing `IS3PresignedUrlGenerator`.

#### Public Properties
- `DefaultTimeout`: Gets the default expiration time for presigned URLs (read-only, type: `TimeSpan`).

#### Public Constructors
```csharp
public S3PresignedUrlGenerator()
public S3PresignedUrlGenerator(TimeSpan defaultTimeout)
```
- **Purpose:** Initializes the generator.
  - Default constructor uses a 30-minute timeout.
  - Parameterized constructor allows a custom timeout.
- **Parameters:**
  - `defaultTimeout`: The custom expiration time for presigned URLs.

#### Public Methods
##### `Sign(Blob blob, TimeSpan? timeout = null)`
```csharp
public string Sign(Blob blob, TimeSpan? timeout = null)
```
- **Purpose:** Generates a presigned URL for an S3 object using a `Blob`.
- **Parameters:**
  - `blob`: The `Blob` object to generate a URL for.
  - `timeout`: Optional expiration time (defaults to `DefaultTimeout`).
- **Returns:** A presigned URL string.

##### `Sign(string key, TimeSpan? timeout = null)`
```csharp
public string Sign(string key, TimeSpan? timeout = null)
```
- **Purpose:** Generates a presigned URL for an S3 object using its key.
- **Parameters:**
  - `key`: The S3 key of the object.
  - `timeout`: Optional expiration time (defaults to `DefaultTimeout`).
- **Returns:** A presigned URL string.

---

### 2. `S3FileRequestService`
A service implementing `IFileRequestService` for S3 file operations.

#### Public Constructors
```csharp
public S3FileRequestService(FileUploadSettings settings)
```
- **Purpose:** Initializes the service with S3 settings.
- **Parameters:**
  - `settings`: An instance of `FileUploadSettings` with bucket and region details (typically injected via dependency injection).

#### Public Methods
##### `Bind(string fileKey)`
```csharp
public async Task<Blob> Bind(string fileKey)
```
- **Purpose:** Retrieves a file from a temporary S3 bucket and returns it as a `Blob`.
- **Parameters:**
  - `fileKey`: The key prefix of the file in the temporary bucket.
- **Returns:** A `Blob` object containing the file content and name.
- **Throws:**
  - `Exception` if no file or multiple files are found in the specified folder.

##### `CreateDownloadAction(byte[] data, string filename)`
```csharp
public async Task<object> CreateDownloadAction(byte[] data, string filename)
```
- **Purpose:** Uploads a file to S3 and returns a public download URL.
- **Parameters:**
  - `data`: The file content as a byte array.
  - `filename`: The name of the file.
- **Returns:** An anonymous object with a `Download` property containing the URL.

##### `DeleteTempFiles(TimeSpan _)`
```csharp
public Task DeleteTempFiles(TimeSpan _)
```
- **Purpose:** Intended to delete temporary files (not implemented).
- **Parameters:**
  - `_`: A `TimeSpan` parameter (ignored).
- **Throws:** `InvalidOperationException` with message "S3 should take care of this."

##### `Download(string key)`
```csharp
public Task<ActionResult> Download(string key)
```
- **Purpose:** Intended to download a file (not implemented).
- **Parameters:**
  - `key`: The S3 key of the file.
- **Throws:** `InvalidOperationException` with message "Client should download from S3."

##### `TempSaveUploadedFile(IFormFile file, bool allowUnsafeExtension = false)`
```csharp
public Task<object> TempSaveUploadedFile(IFormFile file, bool allowUnsafeExtension = false)
```
- **Purpose:** Intended to save an uploaded file temporarily (not implemented).
- **Parameters:**
  - `file`: The uploaded file.
  - `allowUnsafeExtension`: Whether to allow unsafe file extensions (default: `false`).
- **Throws:** `InvalidOperationException` with message "Client should upload to S3 directly."

---

### 3. `S3FileProvider`
Implements `IFileProvider` for accessing S3 as a file system.

#### Public Constructors
```csharp
public S3FileProvider(IAmazonS3 amazonS3, string bucketName)
```
- **Purpose:** Initializes the provider with an S3 client and bucket.
- **Parameters:**
  - `amazonS3`: An `IAmazonS3` client instance.
  - `bucketName`: The name of the S3 bucket.

#### Public Methods
##### `GetDirectoryContents(string subpath)`
```csharp
public IDirectoryContents GetDirectoryContents(string subpath)
```
- **Purpose:** Lists the contents of an S3 directory.
- **Parameters:**
  - `subpath`: The path within the bucket (relative to the root).
- **Returns:** An `IDirectoryContents` object (e.g., `S3DirectoryContents`).
- **Throws:** `ArgumentNullException` if `subpath` is null.

##### `GetFileInfo(string subpath)`
```csharp
public IFileInfo GetFileInfo(string subpath)
```
- **Purpose:** Retrieves metadata for an S3 file.
- **Parameters:**
  - `subpath`: The path to the file within the bucket.
- **Returns:** An `IFileInfo` object (e.g., `S3FileInfo`).
- **Throws:** `ArgumentNullException` if `subpath` is null.

##### `Watch(string filter)`
```csharp
public IChangeToken Watch(string filter)
```
- **Purpose:** Monitors changes (not supported in S3).
- **Parameters:**
  - `filter`: The filter pattern (ignored).
- **Returns:** A `NullChangeToken` (no change monitoring).

##### `Dispose()`
```csharp
public void Dispose()
```
- **Purpose:** Disposes the underlying S3 client.

---

### 4. `S3FileInfo`
Implements `IFileInfo` for S3 file metadata.

#### Public Properties
- `Exists`: `bool` indicating if the file exists in S3.
- `Length`: `long` representing the file size in bytes.
- `Name`: `string` containing the file name.
- `PhysicalPath`: `string` with the S3 URL (e.g., `s3-region.amazonaws.com/bucket/key`).
- `LastModified`: `DateTimeOffset` of the last modification.
- `IsDirectory`: `bool` indicating if the key is a directory (ends with `/`).

#### Public Methods
##### `CreateReadStream()`
```csharp
public Stream CreateReadStream()
```
- **Purpose:** Provides a stream to read the file content.
- **Returns:** A `Stream` object.

---

### 5. `S3BlobStorageProvider`
Implements `IBlobStorageProvider` for S3 storage operations.

#### Public Methods
##### `CostsToCheckExistence()`
```csharp
public bool CostsToCheckExistence()
```
- **Purpose:** Indicates if checking existence incurs a cost.
- **Returns:** `true` (S3 operations have a cost).

##### `SaveAsync(Blob document)`
```csharp
public async Task SaveAsync(Blob document)
```
- **Purpose:** Uploads a `Blob` to S3.
- **Parameters:**
  - `document`: The `Blob` to save.
- **Throws:** `Exception` if the upload fails.

##### `FileExistsAsync(Blob document)`
```csharp
public async Task<bool> FileExistsAsync(Blob document)
```
- **Purpose:** Checks if a `Blob` exists in S3.
- **Parameters:**
  - `document`: The `Blob` to check.
- **Returns:** `true` if the file exists, `false` if not.

##### `LoadAsync(Blob document)`
```csharp
public async Task<byte[]> LoadAsync(Blob document)
```
- **Purpose:** Loads a `Blob` from S3 as a byte array.
- **Parameters:**
  - `document`: The `Blob` to load.
- **Returns:** A byte array of the file content.
- **Throws:** `Exception` with error logging if loading fails.

##### `DeleteAsync(Blob document)`
```csharp
public async Task DeleteAsync(Blob document)
```
- **Purpose:** Deletes a `Blob` from S3.
- **Parameters:**
  - `document`: The `Blob` to delete.
- **Throws:** `Exception` if deletion fails.

---

### 6. `BlobAWSExtensions`
Extension methods for integrating S3 functionality.

#### Public Methods
##### `AddS3BlobStorageProvider(this IServiceCollection @this, TimeSpan PresignedUrlTimeout)`
```csharp
public static IServiceCollection AddS3BlobStorageProvider(this IServiceCollection @this, TimeSpan PresignedUrlTimeout)
```
- **Purpose:** Registers `S3BlobStorageProvider` and `S3PresignedUrlGenerator` with a custom timeout.
- **Parameters:**
  - `@this`: The `IServiceCollection` instance.
  - `PresignedUrlTimeout`: Timeout for presigned URLs.
- **Returns:** The updated `IServiceCollection`.

##### `AddS3BlobStorageProvider(this IServiceCollection @this)`
```csharp
public static IServiceCollection AddS3BlobStorageProvider(this IServiceCollection @this)
```
- **Purpose:** Registers `S3BlobStorageProvider` and `S3PresignedUrlGenerator` with default settings.
- **Parameters:**
  - `@this`: The `IServiceCollection` instance.
- **Returns:** The updated `IServiceCollection`.

##### `AddS3FileRequestService(this IServiceCollection @this)`
```csharp
public static IServiceCollection AddS3FileRequestService(this IServiceCollection @this)
```
- **Purpose:** Registers S3 file request services (`S3FileRequestService`, `S3FileUploadMarkupGenerator`).
- **Parameters:**
  - `@this`: The `IServiceCollection` instance.
- **Returns:** The updated `IServiceCollection`.

##### `GetS3PresignedUrl(this Blob document, Action<GetPreSignedUrlRequest> config = null)`
```csharp
public static string GetS3PresignedUrl(this Blob document, Action<GetPreSignedUrlRequest> config = null)
```
- **Purpose:** Generates a presigned URL for a `Blob` with customizable request configuration.
- **Parameters:**
  - `document`: The `Blob` to generate a URL for.
  - `config`: Optional action to configure the `GetPreSignedUrlRequest`.
- **Returns:** A presigned URL string.

---

## Usage Example
```csharp
// Setup services 
services.AddS3BlobStorageProvider(TimeSpan.FromMinutes(60));
services.AddS3FileRequestService();

var provider = Context.Current;

// Upload a file
var blob = new Blob(File.ReadAllBytes("example.txt"), "example.txt");
var storageProvider = provider.GetService<IBlobStorageProvider>();
await storageProvider.SaveAsync(blob);

// Check existence
var exists = await storageProvider.FileExistsAsync(blob);

// Load content
var content = await storageProvider.LoadAsync(blob);

// Generate presigned URL
var urlGenerator = provider.GetService<IS3PresignedUrlGenerator>();
var presignedUrl = urlGenerator.Sign(blob);

// Delete file
await storageProvider.DeleteAsync(blob);
```