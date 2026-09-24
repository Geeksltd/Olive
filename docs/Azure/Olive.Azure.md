# Olive.Azure

## Overview
This documentation explains how to use Azure App Configuration for managing secrets using the `Secrets` class in the `Olive.Azure` namespace. The configuration secrets are retrieved from Azure App Configuration and loaded into the application settings.

## Configuration
Before using the `Secrets` class, ensure that the following settings are correctly configured in your application:

- **Azure:Secrets:Key** - The key identifier for the secret in Azure App Configuration.
- **Azure:Secrets:Endpoint** - The endpoint URL for your Azure App Configuration instance.

Example configuration in `appsettings.json`:
```json
{
  "Azure": {
    "Secrets": {
      "Key": "MySecretKey",
      "Endpoint": "https://my-app-config.azconfig.io"
    }
  }
}
```

## Secrets Class
The `Secrets` class is responsible for retrieving secrets stored in Azure App Configuration.

---

## Azure Extensions
The `AzureExtensions` class provides additional methods for working with Azure configurations.

### **Methods**
#### `LoadAzureSecrets`
```csharp
public static void LoadAzureSecrets(this IConfiguration @this)
```
Loads secret information from Azure App Configuration. This method initializes an instance of the `Secrets` class and calls `Load()` to retrieve secrets.

Example usage:
```csharp
IConfiguration configuration = new ConfigurationBuilder().Build();
configuration.LoadAzureSecrets();
```

## Error Handling
If the necessary settings are missing, the following exceptions may occur:
- `ArgumentNullException` if `Azure:Secrets:Key` or `Azure:Secrets:Endpoint` is not found.
- `RequestFailedException` if there is an issue with retrieving secrets from Azure.

## Notes
- Ensure that your application has the correct Azure Identity permissions to access the App Configuration service.
- Use Managed Identity or service principals for authentication.

## Conclusion
The `Secrets` class and `LoadAzureSecrets` method provide a seamless way to integrate Azure App Configuration secrets into your application, ensuring secure and centralized configuration management.

