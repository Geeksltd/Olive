# Olive.Security.Cloud.CustomEncryption

This document provides an overview and usage examples for the public classes and methods in the `Olive.Security.Cloud.CustomEncryption` namespace. It implements a custom encryption-based key management and data protection service using AES encryption, integrated with ASP.NET Core's data protection system. A configuration section details the required environment variable setting.

---

## Table of Contents

1. [DataKeyService](#datakeyservice)
   - [Overview](#datakeyservice-overview)
   - [Methods](#datakeyservice-methods)
2. [CustomEncryptionDataProtectionProvider](#customencryptiondataprotectionprovider)
   - [Overview](#customencryptiondataprotectionprovider-overview)
   - [Methods](#customencryptiondataprotectionprovider-methods)
3. [Configuration](#configuration)

---

## DataKeyService

### DataKeyService Overview

The `DataKeyService` class implements `IDataKeyService` to generate and retrieve encryption keys using AES encryption with a master key sourced from an environment variable. It uses a fixed IV derived from the master key and caches keys for efficiency.

### DataKeyService Methods

- **`GenerateKey()`**
  - Asynchronously generates a new encryption key (a GUID without hyphens) and encrypts it using AES with the master key.
  - **Usage Example:**
    ```csharp
    var keyService = new DataKeyService();
    var key = await keyService.GenerateKey();
    Console.WriteLine($"Generated key length: {key.EncryptionKey.Length} bytes");
    ```

- **`GetEncryptionKey(byte[] encryptionKeyReference)`**
  - Decrypts an encryption key from its reference (encrypted blob) using AES with the master key.
  - **Usage Example:**
    ```csharp
    var keyService = new DataKeyService();
    var key = await keyService.GenerateKey();
    var encryptionKey = keyService.GetEncryptionKey(key.EncryptionKeyReference);
    Console.WriteLine($"Retrieved key length: {encryptionKey.Length} bytes");
    ```

---

## CustomEncryptionDataProtectionProvider

### CustomEncryptionDataProtectionProvider Overview

The `CustomEncryptionDataProtectionProvider` class extends `Cloud.DataProtectionProvider<DataKeyService>` to provide data protection services using the custom AES-based encryption implemented in `DataKeyService`. It integrates with ASP.NET Core's data protection stack.

### CustomEncryptionDataProtectionProvider Methods

- **`CreateProtector(string purpose)`**
  - Creates a new data protector instance for the specified purpose, leveraging the custom encryption service.
  - **Usage Example:**
    ```csharp
    var provider = new CustomEncryptionDataProtectionProvider();
    var protector = provider.CreateProtector("MyPurpose");
    var protectedData = protector.Protect(System.Text.Encoding.UTF8.GetBytes("Sensitive data"));
    Console.WriteLine($"Protected data: {Convert.ToBase64String(protectedData)}");
    ```

---

## Configuration

The `DataKeyService` class requires a specific configuration setting, which is sourced from an environment variable rather than an `appsettings.json` file in this implementation. Below is the required setting:

### Required Setting
- **`Encryption:MasterKey`** (Environment Variable)
  - The master key used for AES encryption and decryption. It must be a valid string that can be converted to bytes using UTF-8 encoding.
  - **Note:** The key should ideally be at least 32 characters (for a 256-bit key) to match AES-256 requirements, though the code will use whatever length is provided and derive a 16-byte IV from it.

### Notes
- The `Encryption:MasterKey` environment variable is mandatory. If it is not set or empty, an exception will be thrown with the message "Could not find the master key."
- The AES implementation uses the master key directly as the encryption key and derives a 16-byte initialization vector (IV) from the first 16 bytes of the key. For security, ensure the master key is sufficiently random and long (e.g., 32 bytes for AES-256).