# Olive.Security.Cloud

This document provides an overview and usage examples for the public classes and methods in the `Olive.Security.Cloud` namespace. It defines a foundational framework for cloud-based data protection and key management, abstracting key generation and encryption/decryption operations. A configuration section notes that specific settings depend on the concrete implementation of `IDataKeyService`.

---

## Table of Contents

1. [DataProtectionProvider<TDataKeyService>](#dataprotectionprovider)
   - [Overview](#dataprotectionprovider-overview)
   - [Methods](#dataprotectionprovider-methods)
2. [IDataKeyService](#idatakeyservice)
   - [Overview](#idatakeyservice-overview)
   - [Methods](#idatakeyservice-methods)
3. [Key](#key)
   - [Overview](#key-overview)
4. [Configuration](#configuration)

---

## DataProtectionProvider<TDataKeyService>

### DataProtectionProvider Overview

The `DataProtectionProvider<TDataKeyService>` abstract class implements `IDataProtector` to provide data protection services using a specified `IDataKeyService` implementation. It generates encryption keys, protects data with a symmetric key protector, and caches decrypted data for efficiency, incorporating GZip compression for protected data.

### DataProtectionProvider Methods

- **`CreateProtector(string purpose)`**
  - Abstract method to create a new data protector instance for the specified purpose. Must be implemented by derived classes.  

- **`Protect(byte[] plaintext)`**
  - Protects (encrypts) the provided plaintext data using a generated key and GZip compression.
  - **Usage Example:**
    ```csharp
    var provider = new MyProvider(); // Assuming MyProvider and MyDataKeyService are implemented
    var plaintext = System.Text.Encoding.UTF8.GetBytes("Sensitive data");
    var protectedData = provider.Protect(plaintext);
    Console.WriteLine($"Protected data: {Convert.ToBase64String(protectedData)}");
    ```

- **`Unprotect(byte[] protectedData)`**
  - Unprotects (decrypts) the provided protected data, decompressing it from GZip and caching the result.
  - **Usage Example:**
    ```csharp
    var provider = new MyProvider(); // Assuming MyProvider and MyDataKeyService are implemented
    var plaintext = System.Text.Encoding.UTF8.GetBytes("Sensitive data");
    var protectedData = provider.Protect(plaintext);
    var unprotectedData = provider.Unprotect(protectedData);
    Console.WriteLine($"Unprotected data: {System.Text.Encoding.UTF8.GetString(unprotectedData)}");
    ```

---

## IDataKeyService

### IDataKeyService Overview

The `IDataKeyService` interface defines a contract for key management services, providing methods to generate and retrieve encryption keys. Concrete implementations (e.g., AWS KMS, Azure Key Vault) must provide the actual logic.

### IDataKeyService Methods

- **`GenerateKey()`**
  - Asynchronously generates a new encryption key and its reference.
  - **Usage Example:** (Assuming a concrete implementation)
    ```csharp
    public class MyDataKeyService : IDataKeyService
    {
        public async Task<Key> GenerateKey() => new Key { EncryptionKey = new byte[16], EncryptionKeyReference = new byte[16] };
        public byte[] GetEncryptionKey(byte[] reference) => new byte[16];
    }

    var service = new MyDataKeyService();
    var key = await service.GenerateKey();
    ```

- **`GetEncryptionKey(byte[] encryptionKeyReference)`**
  - Retrieves the encryption key corresponding to the provided reference.
  - **Usage Example:** (Assuming a concrete implementation)
    ```csharp
    var service = new MyDataKeyService();
    var key = await service.GenerateKey();
    var encryptionKey = service.GetEncryptionKey(key.EncryptionKeyReference);
    ```

---

## Key

### Key Overview

The `Key` class represents a key pair consisting of an encryption key and its encrypted reference, used by `IDataKeyService` implementations to manage encryption operations.

- **Usage Example:**
  ```csharp
  var key = new Key
  {
      EncryptionKey = System.Text.Encoding.UTF8.GetBytes("my-encryption-key"),
      EncryptionKeyReference = System.Text.Encoding.UTF8.GetBytes("encrypted-ref")
  };
  Console.WriteLine($"Key length: {key.EncryptionKey.Length}");
  ```

---

## Configuration

The `DataProtectionProvider<TDataKeyService>` class itself does not directly require configuration settings, as it relies on the specific `TDataKeyService` implementation for key management. Therefore, configuration requirements vary depending on the concrete `IDataKeyService` implementation used (e.g., AWS KMS, Azure Key Vault, custom encryption).

 ### Notes
- To use this library, you must instantiate a concrete `DataProtectionProvider` with an appropriate `IDataKeyService` implementation and ensure its configuration is provided.