# Olive.Security.Azure

This document provides an overview and usage examples for the public classes and methods in the `Olive.Security.Azure` namespace. It integrates Azure Key Vault for key management and data protection, including key generation and encryption, as well as persisting data protection keys to Azure Blob Storage. A configuration section details the required settings in an `appsettings.json` file or environment variables.

---

## Table of Contents

1. [DataKeyService](#datakeyservice)
   - [Overview](#datakeyservice-overview)
   - [Methods](#datakeyservice-methods)
2. [Extensions](#extensions)
   - [Overview](#extensions-overview)
   - [Methods](#extensions-methods)
3. [KeyVaultDataProtectionProvider](#keyvaultdataprotectionprovider)
   - [Overview](#keyvaultdataprotectionprovider-overview)
   - [Methods](#keyvaultdataprotectionprovider-methods)
4. [Configuration](#configuration)

---

## DataKeyService

### DataKeyService Overview

The `DataKeyService` class implements `IDataKeyService` to generate and retrieve encryption keys using Azure Key Vault. It caches decrypted keys for efficient reuse and uses the Azure Key Vault KeyClient and CryptographyClient for key operations.

### DataKeyService Methods

- **`GenerateKey()`**
  - Asynchronously generates a new encryption key and wraps it using Azure Key Vault with the RSA1_5 algorithm.
  - **Usage Example:**
    ```csharp
    var keyService = new DataKeyService();
    var key = await keyService.GenerateKey();
    Console.WriteLine($"Generated key length: {key.EncryptionKey.Length} bytes");
    ```

- **`GetEncryptionKey(byte[] encryptionKeyReference)`**
  - Retrieves or unwraps an encryption key from a given reference (encrypted key blob) using Azure Key Vault, caching the result.
  - **Usage Example:**
    ```csharp
    var keyService = new DataKeyService();
    var key = await keyService.GenerateKey();
    var encryptionKey = keyService.GetEncryptionKey(key.EncryptionKeyReference);
    Console.WriteLine($"Retrieved key length: {encryptionKey.Length} bytes");
    ```

---

## Extensions

### Extensions Overview

The `Extensions` static class provides an extension method to configure ASP.NET Core data protection to persist keys to Azure Blob Storage.

### Extensions Methods

- **`PersistKeysToAzureBlobStorage(this IDataProtectionBuilder builder)`**
  - Configures the data protection system to persist keys to Azure Blob Storage using a default Azure credential.
  - **Usage Example:**
    ```csharp
    IDataProtectionBuilder builder;
    builder.PersistKeysToAzureBlobStorage();     
    ```

---

## KeyVaultDataProtectionProvider

### KeyVaultDataProtectionProvider Overview

The `KeyVaultDataProtectionProvider` class extends `Cloud.DataProtectionProvider<DataKeyService>` to provide data protection services using Azure Key Vault keys. It integrates with ASP.NET Core's data protection stack.

### KeyVaultDataProtectionProvider Methods

- **`CreateProtector(string purpose)`**
  - Creates a new data protector instance for the specified purpose, leveraging Azure Key Vault for key management.
  - **Usage Example:**
    ```csharp
    var provider = new KeyVaultDataProtectionProvider();
    var protector = provider.CreateProtector("MyPurpose");
    var protectedData = protector.Protect(System.Text.Encoding.UTF8.GetBytes("Sensitive data"));
    Console.WriteLine($"Protected data: {Convert.ToBase64String(protectedData)}");
    ```

---

## Configuration

The `DataKeyService` and related classes require specific configuration settings, which can be stored in an `appsettings.json` file or as environment variables. Below are the required settings:

### Required Settings
- **`Azure:KeyVault:CookieAuthentication:Uri`** (or Environment Variable `AZURE_KEY_VALUT_COOKIE_AUTHENTICATION_URI`)
  - The URI of the Azure Key Vault (e.g., `https://your-vault-name.vault.azure.net/`).
  - **Priority:** The environment variable takes precedence over the `appsettings.json` value if both are present.
- **`Azure:KeyVault:CookieAuthentication:KeyName`** (or Environment Variable `AZURE_KEY_VALUT_COOKIE_AUTHENTICATION_KEY_NAME`)
  - The name of the master key in Azure Key Vault used for wrapping and unwrapping encryption keys.
  - **Priority:** The environment variable takes precedence over the `appsettings.json` value if both are present.
- **`Azure:KeyVault:CookieAuthentication:BlobStorageUri`** (or Environment Variable `AZURE_KEY_VALUT_COOKIE_AUTHENTICATION_BLOB_STORAGE_URI`)
  - The URI of the Azure Blob Storage container for persisting data protection keys (e.g., `https://your-storage-account.blob.core.windows.net/your-container/`).
  - **Priority:** The environment variable takes precedence over the `appsettings.json` value if both are present.

### Full `appsettings.json` Example
```json
{
  "Azure": {
    "KeyVault": {
      "CookieAuthentication": {
        "Uri": "https://your-vault-name.vault.azure.net/",
        "KeyName": "DataProtectionKey",
        "BlobStorageUri": "https://your-storage-account.blob.core.windows.net/your-container/"
      }
    }
  }
}
```

 

### Notes
- All three settings (`Uri`, `KeyName`, `BlobStorageUri`) are mandatory. If any are missing, an exception will be thrown (e.g., "Azure Key Valut Authentication uri is not specified").
- The application must have appropriate Azure credentials configured (e.g., via `DefaultAzureCredential`), which may use environment variables (`AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET`, `AZURE_TENANT_ID`), managed identity, or other authentication methods supported by Azure.Identity.
- The Azure Key Vault key must support wrapping/unwrapping operations (e.g., RSA key type), and the Blob Storage container must be accessible with write permissions.