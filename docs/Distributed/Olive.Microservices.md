# Olive.Microservices

## Overview

The **Olive.Microservices** library simplifies inter-service communication within Olive-based microservices architecture. It provides utility classes and extension methods to connect, configure, and invoke operations or APIs across services easily and consistently.

Using Olive Microservice, configuring microservices URLs, handling authentication tokens, accessing resources, interacting with APIs (via ApiClient), and applying caching and resilience strategies is intuitive.

---

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Configuration](#configuration)
- [Core Classes and Methods](#core-classes-and-methods)
  - [Microservice Class](#microservice-class)
- [Working with ApiClient](#working-with-apiclient)
  - [API Authentication and Headers](#api-authentication-and-headers)
- [Caching and Resilience Strategies](#caching-and-resilience-strategies)
  - [Caching with CachePolicy](#caching-with-cachepolicy)
  - [Circuit Breaker](#circuit-breaker-pattern)
  - [Retries Logic](#retries-logic)
- [Usage Examples](#usage-examples)
- [Dependencies](#dependencies)
- [Important Notes](#important-notes)

---

## Features

- Simplifies discovery and referencing of services using URL-based configurations.
- Supports easy creation of `ApiClient` instances with service authentication handling.
- Provides clear configuration management options for resources and S3 buckets.
- Integrated caching strategies to optimize API performance and resilience strategies (circuit breakers, retries) for fault tolerance.

---

## Installation

Install the Olive libraries using NuGet:

```powershell
Install-Package Olive
```

Ensure you also have required dependencies such as:

- `Microsoft.Extensions.Hosting`
- `Microsoft.Extensions.DependencyInjection`

---

## Configuration

Configure microservices URLs and keys in `appsettings.json`:

```json
{
  "Authentication": {
    "Cookie": { "Domain": "https://yourapp.com" }
  },
  "Aws": { "Region": "eu-west-1" },
  "Blob": {
    "S3": {
      "Bucket": "your-bucket",
      "Region": "eu-west-1"
    }
  },
  "Microservice": {
    "Posts": {
      "Url": "https://posts.yourapp.com/",
      "S3BucketUrl": "https://bucket-url/",
      "AccessKey": "SERVICE_ACCESS_TOKEN"
    },
    "Identity": {
      "Url": "https://identity.yourapp.com/"
    },
    "Me": {
      "Name": "MyService",
      "Url": "https://myservice.yourapp.com/"
    }
  }
}
```

---

## Core Classes and Methods

### Microservice Class

The `Microservice` class encapsulates logic for:

- Discovering URLs to access other services.
- Creating ApiClient with authentication headers.
- Retrieving resource and asset URLs for services.

#### Main Properties:
- `Name` - Application service identifier.
- URL access: `Url`, `GetResourceUrl`, `GetS3BucketUrl`

### Factory Methods:
- `Microservice.Of("service name")` to get another service instance.
- `Microservice.Me` to get currently executing service.

Example usage:

```csharp
var identityService = Microservice.Of("Identity");

// get full URL for specific path
var loginUrl = identityService.Url("Account/Login");
```
 
---

## Working with ApiClient

`ApiClient` makes invoking microservice APIs easy and streamlined, handling common concerns such as authentication headers automatically.

#### API Authentication and Headers

Example of creating a secured API client:

```csharp
var apiClient = Microservice.Of("Identity").Api("Account/GetUser");
```

The above setup automatically adds an authentication header (`Microservice.AccessKey`) if configured.

---

## Caching and Resilience Strategies

Enhance your microservice robustness using caching mechanisms and resilience patterns provided out-of-box.

### Caching with CachePolicy

Set caching strategies using `CachePolicy`:

```csharp
// Cached result up to 5 minutes
proxy.Cache(CachePolicy.FreshOrCache, TimeSpan.FromMinutes(5));
```

### Circuit Breaker Pattern

Circuit breakers help isolate failing services. If a service repeatedly fails, it temporarily stops making requests:

```csharp
proxy.CircuitBreaker(exceptionsBeforeBreaking: 3, breakDurationSeconds: 15);
```

This prevents cascading failures in microservices architectures.

### Retries Logic

Avoid transient errors by automatically retrying failed requests:

```csharp
proxy.Retries(3, pauseMilliseconds: 500);
```

---

## Usage Examples

### Getting a URL from a Microservice Instance:

```csharp
var postsService = Microservice.Of("Posts");

// Get Resources URL (CSS, JS, Media)
string stylePath = postsService.GetResourceUrl("styles/main.css");

// Get S3 Bucket URL for assets
string logoImage = postsService.GetS3BucketUrl("images/logo.png");
```

### Using ApiClient for making secure API calls:

```csharp
// Create an API Client for "Identity" microservice
var client = Microservice.Of("Identity").Api("Users/GetDetails");

// add custom header if desired
client.Header(h => h.Add("X-Custom-Header", "value"));

var user = await client.Get<UserDetailsDto>();
``` 

---

## Dependencies

Make sure you have the required packages installed before use:

- Olive (`Install-Package Olive`)
- Microsoft Extensions:
  - Hosting (`Microsoft.Extensions.Hosting`)
  - DependencyInjection (`Microsoft.Extensions.DependencyInjection`)

```powershell
Install-Package Microsoft.Extensions.Hosting
Install-Package Microsoft.Extensions.DependencyInjection
```

---

## Exception Handling

- Robust exception handling for missing configurations or invalid URLs.
- Informative logs and exceptions provided for issues such as:

  ```
  "No queue url is specified in either EventBusLoggerOptions or under config key of Logging:EventBus:QueueUrl"
  ```

---

## Important Notes

- Ensure configurations (`appsettings.json`) are properly set up for each microservice entry.
- Secure your access keys and connection details carefully. Utilize secret management or environment variables for sensitive data.
- Circuit Breaker and Retry settings should be chosen carefully based on application requirements and external service reliability.
- Ensure assets or resources are correctly configured with appropriate permissions (e.g., AWS S3) to avoid runtime failures.