# Olive.Security.Auth0

This document provides an overview and usage examples for the public class and methods in the `Olive.Security` namespace related to Auth0 authentication. It enables username/password authentication via the Auth0 Authentication API. A configuration section details the required settings in an `appsettings.json` file.

---

## Table of Contents

1. [Auth0](#auth0)
   - [Overview](#auth0-overview)
   - [Methods](#auth0-methods)
   - [Nested Classes](#auth0-nested-classes)
2. [Configuration](#configuration)

---

## Auth0

### Auth0 Overview

The `Auth0` class provides a static interface to authenticate users against an Auth0 tenant using the `Auth0.AuthenticationApi`. It leverages the Auth0 Authentication API client to perform username/password login requests and returns the result as an `AuthenticationResult`.

### Auth0 Methods

- **`Authenticate(string username, string password)`**
  - Asynchronously authenticates a user with the provided username and password against Auth0.
  - **Usage Example:**
    ```csharp
    var result = await Auth0.Authenticate("user@example.com", "securepassword");
    if (result.Success)
    {
        Console.WriteLine("Authentication successful");
    }
    else
    {
        Console.WriteLine($"Authentication failed: {result.Message}");
    }
    ```

### Auth0 Nested Classes

- **`AuthenticationResult`**
  - Represents the outcome of an authentication attempt.
  - **Usage Example:**
    ```csharp
    var result = await Auth0.Authenticate("user@example.com", "wrongpassword");
    Console.WriteLine($"Success: {result.Success}, Message: {result.Message}");
    // Outputs: Success: False, Message: <Auth0 error message>
    ```

---

## Configuration

The `Auth0` class requires specific configuration settings stored in an `appsettings.json` file with a JSON structure. Below are the required settings:

### Required Settings
- **`Authentication:Auth0:ClientId`**
  - The Client ID of your Auth0 application, obtained from the Auth0 dashboard.
- **`Authentication:Auth0:ClientSecret`**
  - The Client Secret of your Auth0 application, obtained from the Auth0 dashboard.
- **`Authentication:Auth0:Domain`**
  - The domain of your Auth0 tenant (e.g., `your-tenant.auth0.com`).

### Full `appsettings.json` Example
```json
{
  "Authentication": {
    "Auth0": {
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret",
      "Domain": "your-tenant.auth0.com"
    }
  }
}
```

### Notes
- All three settings (`ClientId`, `ClientSecret`, `Domain`) are mandatory. If any are missing, the `Config.Get` method will throw an exception unless default values are handled elsewhere in your application.
- The Auth0 credentials must correspond to an application configured with the "Username-Password-Authentication" connection enabled in your Auth0 tenant.