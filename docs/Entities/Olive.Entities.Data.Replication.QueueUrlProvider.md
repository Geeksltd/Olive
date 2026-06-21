# Olive.Entities.Data.Replication.QueueUrlProvider

## Overview

The Olive Framework provides a specialized utility for retrieving replication queue URLs dynamically through configuration. The `UrlProvider` class centralizes URL resolution logic for replication endpoints, facilitating cleaner and more maintainable configuration across various environments and deployments.

---

## Table of Contents

- [Class Overview](#class-overview)
- [Methods](#methods)
- [Usage Examples](#usage-examples)
- [Configuration](#configuration)
- [Notes](#notes)  

---

## Class Overview  

### `UrlProvider` Class  

The `UrlProvider` class simplifies retrieval of queue URLs for Olive's data replication endpoints. It reads configuration settings dynamically based on given endpoint types and provides default behavior to assist during development.

---

## Methods  

### `GetUrl`

Retrieves the URL configured for a specified replication endpoint type from configuration. It attempts multiple configuration key patterns and falls back to a clearly marked development-only URL if none are configured.

#### Signature:

```csharp
public static string GetUrl(Type type);
```

#### Behavioral Details:

- **First attempt:** Tries to fetch from configuration with key pattern:
  ```
  DataReplication:{Full_Type_Name_With_Underscores}:Url
  ```
- **Second attempt (fallback):** Tries to fetch from configuration with key pattern:
  ```
  DataReplication:{Full.Type.Name}:Url
  ```
- **Default/Development fallback** (if both attempts fail):
  ```
  FOR_DEVELOPMENT_ONLY_DataReplication_{Full.Type.Name}
  ```

#### Example keys it attempts:

For type `My.Namespace.UserEndpoint`:
```
DataReplication:My_Namespace_UserEndpoint:Url
DataReplication:My.Namespace.UserEndpoint:Url
```

---

## Usage Examples  

### Obtaining a URL dynamically for replication:

```csharp
Type endpointType = typeof(MyNamespace.MyUserEndpoint);
string replicationUrl = UrlProvider.GetUrl(endpointType);
```

_If configured correctly, returns something like:_

```
https://sqs.region.amazonaws.com/1234567890/my-user-endpoint
```

If nothing is configured (_development scenario_), it returns:

```
FOR_DEVELOPMENT_ONLY_DataReplication_MyNamespace.MyUserEndpoint
```

---

## Configuration  

In your application's `appsettings.json` or other configuration sources, you should set the Queue URL to connect your replication endpoints:

### Example configuration:

```json
{
  "DataReplication": {
    "My_Namespace_UserEndpoint": {
      "Url": "https://sqs.region.amazonaws.com/1234567890/my-user-endpoint"
    },
    "My.Namespace.UserEndpoint": {
      "Url": "https://sqs.region.amazonaws.com/1234567890/my-user-endpoint-alt"
    }
  }
}
```

**Note**:  
Either configuration pattern (`_` or `.`) can be used. The provider checks both, starting with the underscore replacement convention first.

---

## Notes  

- The "FOR_DEVELOPMENT_ONLY" default prefix highlights clearly when URLs are not configured, assisting accurate identification and avoidance of unintended production deployments.
- It is recommended to configure real environment-specific URLs for staging and production environments explicitly in your configuration files.
- UrlProvider centralizes logic for URL retrieval, ensuring consistency across your replication management solutions.