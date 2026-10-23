# Olive.Blob.Azure

This document outlines the public methods, variables, and properties in the `Olive.Blob.Azure` namespace, designed to help users interact with Azure Blob Storage for file storage, retrieval, and management.

--- 

### 1. `SASTokenGenerator`
A class to generate SAS (Shared Access Signature) tokens for Azure Blob Storage containers.
 
##### `GenerateContainerSasUri(string storageContainerName = null)`
```csharp
public Uri GenerateContainerSasUri(string storageContainerName = null)
```
- **Purpose:** Generates a SAS URI for an Azure Blob container with read, write, create, add, delete, and list permissions.
- **Parameters:**
  - `storageContainerName`: Optional container name (defaults to the configured `AzureBlobInfo.StorageContainer` if not provided).
- **Returns:** A `Uri` object representing the SAS URI, valid for 8 hours from generation.

---

### 2. `BlobAzureExtension`
Extension methods for integrating Azure Blob Storage into a service collection.

#### Public Methods
##### `AddAzureBlobStorageProvider(this IServiceCollection @this)`
```csharp
public static IServiceCollection AddAzureBlobStorageProvider(this IServiceCollection @this)
```
- **Purpose:** Registers `AzureBlobStorageProvider` as an `IBlobStorageProvider` in the dependency injection container.
- **Parameters:**
  - `@this`: The `IServiceCollection` instance to extend. 
  
---

### 3. `AzureBlobStorageProvider`
Implements `IBlobStorageProvider` for Azure Blob Storage operations.

#### Public Constructors
```csharp
public AzureBlobStorageProvider()
public AzureBlobStorageProvider(BlobServiceClient blobServiceClient)
public AzureBlobStorageProvider(BlobServiceClient blobServiceClient, string containerName)
```
- **Purpose:** Initializes the provider.
  - Default constructor uses the connection string from `AzureBlobInfo.StorageConnectionString`.
  - Parameterized constructors allow a custom `BlobServiceClient` and optional container name.
- **Parameters:**
  - `blobServiceClient`: An instance of `BlobServiceClient`.
  - `containerName`: Optional custom container name (defaults to `AzureBlobInfo.StorageContainer`).
 
##### `CostsToCheckExistence()`
```csharp
public bool CostsToCheckExistence()
```
- **Purpose:** Indicates if checking file existence incurs a cost.
- **Returns:** `true` (Azure Blob operations have a cost).

##### `SaveAsync(Blob document)`
```csharp
public async Task SaveAsync(Blob document)
```
- **Purpose:** Uploads a `Blob` to Azure Blob Storage.
- **Parameters:**
  - `document`: The `Blob` object to save.
- **Throws:** `Exception` with debug logging if the upload fails.

##### `SaveAsync(Blob document, string key)`
```csharp
public async Task SaveAsync(Blob document, string key)
```
- **Purpose:** Uploads a `Blob` to Azure Blob Storage with a specified key.
- **Parameters:**
  - `document`: The `Blob` object to save.
  - `key`: The custom key to use (defaults to `document.GetKey()` if null/empty).
- **Throws:** `Exception` with debug logging if the upload fails.

##### `FileExistsAsync(Blob document)`
```csharp
public async Task<bool> FileExistsAsync(Blob document)
```
- **Purpose:** Checks if a `Blob` exists in Azure Blob Storage.
- **Parameters:**
  - `document`: The `Blob` object to check.
- **Returns:** `true` if the file exists, `false` otherwise.
- **Throws:** `Exception` with error logging if the check fails.

##### `FileExistsAsync(Blob document, string key)`
```csharp
public async Task<bool> FileExistsAsync(Blob document, string key)
```
- **Purpose:** Checks if a file exists in Azure Blob Storage with a specified key.
- **Parameters:**
  - `document`: The `Blob` object (used if `key` is null/empty).
  - `key`: The custom key to check (defaults to `document.GetKey()` if null/empty).
- **Returns:** `true` if the file exists, `false` otherwise.
- **Throws:** `Exception` with error logging if the check fails.

##### `LoadAsync(Blob document)`
```csharp
public async Task<byte[]> LoadAsync(Blob document)
```
- **Purpose:** Downloads a `Blob` from Azure Blob Storage as a byte array.
- **Parameters:**
  - `document`: The `Blob` object to load.
- **Returns:** A byte array of the file content.
- **Throws:** `Exception` with error logging if loading fails.

##### `DeleteAsync(Blob document)`
```csharp
public async Task DeleteAsync(Blob document)
```
- **Purpose:** Deletes a `Blob` from Azure Blob Storage.
- **Parameters:**
  - `document`: The `Blob` object to delete.
- **Behavior:** Does nothing if `document` is empty.
- **Throws:** `Exception` with error logging if deletion fails.

##### `DeleteAsync(Blob document, string key)`
```csharp
public async Task DeleteAsync(Blob document, string key)
```
- **Purpose:** Deletes a file from Azure Blob Storage with a specified key.
- **Parameters:**
  - `document`: The `Blob` object (used if `key` is null/empty).
  - `key`: The custom key to delete (defaults to `document.GetKey()` if null/empty).
- **Behavior:** Does nothing if `document` is empty.
- **Throws:** `Exception` with error logging if deletion fails.

---

### 4. `AzureBlobInfo`
A utility class providing Azure Blob Storage configuration details.

#### Public Properties
- `StorageAccountKey`: Gets the storage account key from configuration (read-only, type: `string`).
  - **Source:** Retrieved from `Config.Get<string>("AzureStorage:AccountKey")`.

---

## Usage Example
```csharp
// Setup services 
services.AddAzureBlobStorageProvider();
var provider = Context.Current;

// Get the storage provider
var storageProvider = provider.GetService<IBlobStorageProvider>();

// Upload a file
var blob = new Blob(File.ReadAllBytes("example.txt"), "example.txt");
await storageProvider.SaveAsync(blob);

// Check existence
var exists = await storageProvider.FileExistsAsync(blob);

// Load content
var content = await storageProvider.LoadAsync(blob);

// Generate SAS URI
var sasGenerator = new SASTokenGenerator();
var sasUri = sasGenerator.GenerateContainerSasUri();

// Delete file
await storageProvider.DeleteAsync(blob);
```