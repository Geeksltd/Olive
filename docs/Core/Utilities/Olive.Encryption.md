# Olive.Encryption

## Overview

This document provides details about the encryption, decryption, hashing, and key management utilities available in the `Olive.Security` namespace. The utilities facilitate symmetric encryption (AES), asymmetric encryption (RSA), and secure password hashing using the PBKDF2 algorithm.

---

## Table of Contents
- [Symmetric Encryption (AES)](#symmetric-encryption-aes)
- [Asymmetric Encryption (RSA)](#asymmetric-encryption-rsa)
- [Secure Password Hashing (PBKDF2)](#secure-password-hashing-pbkdf2)
- [Utility Classes and Extensions](#utility-classes-and-extensions)
- [Examples](#examples)

---

## Symmetric Encryption (AES)

This utility enables encrypting and decrypting data symmetrically based on the AES algorithm.

### Methods:

#### Encryption.Encrypt()

Encrypts the specified text or binary data using AES encryption.

Overloads:

- Encrypt string to Base64:
  ```csharp
  string Encrypt(string raw, string password, Encoding encoding = null)
  ```

- Encrypt byte array to byte array:
  ```csharp
  byte[] Encrypt(byte[] raw, string password)
  ```

**Example usage:**
```csharp
var plaintext = "Hello World!";
var password = "SecurePassword123";

string encrypted = Encryption.Encrypt(plaintext, password);
// encrypted now contains the Base64 AES encrypted data.
```

---

#### Encryption.Decrypt()

Decrypts AES-encrypted data back to original plaintext.

Overloads:

- Decrypt Base64 string to plaintext:
  ```csharp
  string Decrypt(string cipher, string password, Encoding encoding = null)
  ```

- Decrypt byte array to original byte array:
  ```csharp
  byte[] Decrypt(byte[] cipher, string password)
  ```

**Example usage:**
```csharp
var decryptedText = Encryption.Decrypt(encrypted, password);
// decryptedText will return original string "Hello World!".
```

---

## Asymmetric Encryption (RSA)

Provides RSA asymmetric cryptography methods, allowing to encrypt data with a public key and decrypt with the corresponding private key.

### Class: Encryption.AsymmetricKeyPair

The `AsymmetricKeyPair` class generates and holds RSA cryptographic keys:

| Property | Description |
| -------- | ----------- |
| `EncryptionKey` | The RSA public key (Base64 string). Use this key to encrypt data. |
| `DecryptionKey` | The RSA private key (Base64 string). Use this to decrypt previously encrypted data. |

### Methods:

#### AsymmetricKeyPair.Generate()

Generates a new RSA public/private key pair.

```csharp
AsymmetricKeyPair keyPair = Encryption.AsymmetricKeyPair.Generate();
```

### RSA Encryption Methods:

- **EncryptAsymmetric**
  Encrypt byte array with RSA public key:

  ```csharp
  byte[] EncryptAsymmetric(byte[] raw, string encryptKey)
  ```

- **DecryptAsymmetric**
  Decrypt byte array with RSA private key:

  ```csharp
  byte[] DecryptAsymmetric(byte[] cipher, string decryptKey)
  ```

**Example usage:**
```csharp
// Generate keys
var keyPair = Encryption.AsymmetricKeyPair.Generate();

byte[] data = Encoding.UTF8.GetBytes("Sensitive Information");

// Encrypt using public key
byte[] encryptedData = Encryption.EncryptAsymmetric(data, keyPair.EncryptionKey);

// Decrypt using private key
byte[] originalData = Encryption.DecryptAsymmetric(encryptedData, keyPair.DecryptionKey);

string originalText = Encoding.UTF8.GetString(originalData); // "Sensitive Information"
```

---

## Secure Password Hashing (PBKDF2)

Implements secure password hashing using the PBKDF2 algorithm with salts and iterations.

### Class: SecurePassword

| Property | Description |
| -------- | ----------- |
| `Password` | Base64-encoded hashed password |
| `Salt`     | Base64-encoded cryptographic salt |

### Methods:

- **SecurePassword.Create**

Creates a salted PBKDF2 hash from a plaintext password.

```csharp
SecurePassword Create(string password)
```

- **SecurePassword.Verify**

Verifies a plaintext password against hashed password and salt.

```csharp
bool Verify(string clearTextPassword, string hashedPassword, string salt)
```

**Example usage:**
```csharp
// Create hashed password:
var securedPass = SecurePassword.Create("MySecurePassword!");

// Verify Password:
bool isPasswordValid = SecurePassword.Verify("MySecurePassword!", securedPass.Password, securedPass.Salt);
```

---

## Utility Classes and Extensions

### Class: EncryptionExtensions

This class provides extension methods for RSA key serialization:

- **ToKey**  
Serializes RSAParameters as a Base64 string.

  ```csharp
  string ToKey(this RSAParameters rsaParams)
  ```

- **FromKey**  
Deserializes RSAParameters from a Base64 string.

  ```csharp
  RSACryptoServiceProvider FromKey(this RSACryptoServiceProvider rsaProvider, string key)
  ```

**Example usage:**
```csharp
var rsa = new RSACryptoServiceProvider();
var rsaParams = rsa.ExportParameters(false);

// Serialize parameters to Base64 string:
string keyString = rsaParams.ToKey();

// Deserialize parameters from string:
rsa = rsa.FromKey(keyString);
```

---

## Examples:

### AES encryption and decryption example:
```csharp
var originalText = "Confidential data";
var password = "SuperSecret123";

// Encrypt
var encrypted = Encryption.Encrypt(originalText, password);

// Decrypt
var decrypted = Encryption.Decrypt(encrypted, password);

// Result:
// decrypted == "Confidential data"
```

### RSA asymmetric encryption and decryption example:
```csharp
// Generate key pair:
var keyPair = Encryption.AsymmetricKeyPair.Generate();
var publicKey = keyPair.EncryptionKey;
var privateKey = keyPair.DecryptionKey;

// Original data:
var originalBytes = Encoding.UTF8.GetBytes("Important data");

// Encryption:
var encryptedData = Encryption.EncryptAsymmetric(originalBytes, publicKey);

// Decryption:
var decryptedData = Encryption.DecryptAsymmetric(encryptedData, privateKey);
var decryptedText = Encoding.UTF8.GetString(decryptedData);

// Result:
// decryptedText == "Important data"
```

### SecurePassword hashing and verification example:
```csharp
// Hash password:
var securePwd = SecurePassword.Create("MyP@ssword!");

// Verify:
var isValid = SecurePassword.Verify("MyP@ssword!", securePwd.Password, securePwd.Salt);

// Result:
// isValid == true
```

---

## Notes:
- Default encodings are UTF-8 if no encoding is specified.
- AES parameters default to `AES_KEY_LENGTH = 32 bytes` and `AES_IV_LENGTH = 16 bytes`.

---

## Exception Handling:
Methods throw `ArgumentNullException` if required parameters are null or empty. For instance:
```csharp
// Throws ArgumentNullException because 'raw' is empty
string encrypted = Encryption.Encrypt("", "password");
```

Please handle exceptions properly in your applications. 