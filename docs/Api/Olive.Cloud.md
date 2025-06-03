# Olive.Cloud

## Overview
The `Olive.Cloud` is an abstract base class designed to manage and load secrets from a cloud-based provider. It stores secrets in a dictionary and updates the application configuration with the retrieved values.

## Namespace
```csharp
namespace Olive.Cloud;
``` 

## Class: `Secrets`
### Description
This abstract class facilitates the retrieval and storage of secrets within an application. The secrets are loaded from a cloud-based provider and stored in memory for use during runtime.

### Events
#### `Loaded`
- **Type:** `AwaitableEventHandler<Secrets>`
- **Description:** This event is triggered when the secrets are successfully loaded.

### Fields
#### `Config`
- **Type:** `IConfiguration`
- **Access Modifier:** `protected`
- **Description:** Holds the application configuration object.

#### `SecretString`
- **Type:** `string`
- **Access Modifier:** `protected`
- **Description:** Stores the downloaded secrets as a JSON string.

### Properties
#### `SecretId`
- **Type:** `string`
- **Access Modifier:** `protected abstract`
- **Description:** Specifies the unique identifier for the secret in the cloud provider.

### Methods
#### `DownloadSecrets()`
- **Access Modifier:** `protected abstract`
- **Return Type:** `string`
- **Description:** An abstract method that must be implemented in derived classes to download secrets from a cloud provider.

#### `Load()`
- **Access Modifier:** `public`
- **Return Type:** `void`
- **Description:** Downloads and loads secrets into the application configuration.
- **Implementation Steps:**
  1. Calls `Download()` to retrieve the secret values.
  2. Parses the retrieved secrets as JSON.
  3. Updates the application configuration with the retrieved secret values.
  4. Raises the `Loaded` event.

#### `Download()`
- **Access Modifier:** `private`
- **Return Type:** `void`
- **Description:** Downloads secrets and handles any errors that occur during retrieval.
- **Error Handling:**
  - Logs errors when secrets cannot be retrieved.
  - Throws exceptions when retrieval fails.
- **Implementation Steps:**
  1. Calls `DownloadSecrets()` to retrieve the secrets.
  2. Validates the retrieved data to ensure it is not empty.
  3. Stores the retrieved secrets in `SecretString`.

### Logging
- Uses `ILogger` for logging errors and exceptions.
- Logs errors when secret retrieval fails.

## Usage Example
Since `Secrets` is an abstract class, it must be inherited by a concrete class that implements `DownloadSecrets()`. Below is an example implementation:

```csharp
public class MyCloudSecrets : Secrets
{
    protected override string SecretId => "MySecretKey";

    public MyCloudSecrets(IConfiguration config) : base(config) {}

    protected override string DownloadSecrets()
    {
        // Replace this with actual cloud secret retrieval logic.
        return "{ \"ApiKey\": \"12345\", \"DbPassword\": \"securepassword\" }";
    }
}
```

To use this class in an application:

```csharp
IConfiguration configuration = new ConfigurationBuilder().Build();
MyCloudSecrets secrets = new MyCloudSecrets(configuration);
secrets.Load();
Console.WriteLine(configuration["ApiKey"]); // Output: 12345
```

