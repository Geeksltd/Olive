# Olive.Security.Aws

This document provides an overview and usage examples for the public classes and methods in the `Olive.Security.Aws` namespace. It integrates AWS Key Management Service (KMS) for generating and managing data encryption keys, as well as providing data protection services. A configuration section details the required settings in an `appsettings.json` file or environment variables.

---

## Table of Contents

1. [DataKeyService](#datakeyservice)
   - [Overview](#datakeyservice-overview)
   - [Methods](#datakeyservice-methods)
2. [KmsDataProtectionProvider](#kmsdataprotectionprovider)
   - [Overview](#kmsdataprotectionprovider-overview)
   - [Methods](#kmsdataprotectionprovider-methods)
3. [Configuration](#configuration)

---

## DataKeyService

### DataKeyService Overview

The `DataKeyService` class implements `IDataKeyService` to generate and retrieve encryption keys using AWS KMS. It caches decrypted keys for efficient reuse and interacts with KMS to generate and decrypt data keys.

### DataKeyService Methods

- **`GenerateKey()`**
  - Asynchronously generates a new AES-256 data key from AWS KMS.
  - **Usage Example:**
    ```csharp
    var keyService = new DataKeyService { MasterKeyArn = "arn:aws:kms:region:account-id:key/key-id" };
    var key = await keyService.GenerateKey();
    Console.WriteLine($"Generated key length: {key.EncryptionKey.Length} bytes");
    ```

- **`GetEncryptionKey(byte[] encryptionKeyReference)`**
  - Retrieves or decrypts an encryption key from a given reference (ciphertext blob) using AWS KMS, caching the result.
  - **Usage Example:**
    ```csharp
    var keyService = new DataKeyService { MasterKeyArn = "arn:aws:kms:region:account-id:key/key-id" };
    var key = await keyService.GenerateKey();
    var encryptionKey = keyService.GetEncryptionKey(key.EncryptionKeyReference);
    Console.WriteLine($"Retrieved key length: {encryptionKey.Length} bytes");
    ```

---

## KmsDataProtectionProvider

### KmsDataProtectionProvider Overview

The `KmsDataProtectionProvider` class extends `Cloud.DataProtectionProvider<DataKeyService>` to provide data protection services using AWS KMS keys. It integrates with ASP.NET Core's data protection stack and configures the master key ARN from configuration or environment variables.

### KmsDataProtectionProvider Methods

- **`CreateProtector(string purpose)`**
  - Creates a new data protector instance for the specified purpose, leveraging AWS KMS for key management.
  - **Usage Example:**
    ```csharp
    var provider = new KmsDataProtectionProvider();
    var protector = provider.CreateProtector("MyPurpose");
    var protectedData = protector.Protect(System.Text.Encoding.UTF8.GetBytes("Sensitive data"));
    Console.WriteLine($"Protected data: {Convert.ToBase64String(protectedData)}");
    ```

---

## Configuration

The `DataKeyService` and `KmsDataProtectionProvider` classes require a specific configuration setting for the AWS KMS master key ARN, which can be stored in an `appsettings.json` file or as an environment variable. Below are the details:

### Required Setting
- **`Aws:Kms:MasterKeyArn`** (or Environment Variable `AWS_KMS_MASTERKEY_ARN`)
  - The Amazon Resource Name (ARN) of the AWS KMS master key used for generating and decrypting data keys.
  - **Priority:** The environment variable `AWS_KMS_MASTERKEY_ARN` takes precedence over the `appsettings.json` value if both are present.

### Full `appsettings.json` Example
```json
{
  "Aws": {
    "Kms": {
      "MasterKeyArn": "arn:aws:kms:us-east-1:123456789012:key/abcd1234-abcd-1234-abcd-1234567890ab"
    }
  }
}
``` 

### Notes
- The master key ARN must be specified either in `appsettings.json` under `Aws:Kms:MasterKeyArn` or as the `AWS_KMS_MASTERKEY_ARN` environment variable. If neither is provided, an exception will be thrown with the message "Aws Master Key Arn is not specified."
- Ensure the AWS credentials used by the application have permissions to call `GenerateDataKey` and `Decrypt` on the specified KMS key.
