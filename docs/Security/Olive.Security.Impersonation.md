# Olive.Security.Impersonation

This document provides an overview and usage examples for the public classes and methods in the `Olive.Security` namespace related to user impersonation. It enables an admin user to impersonate another user, manage the impersonation session, and retrieve impersonation-related information in an ASP.NET Core application.

---

## Table of Contents

1. [ImpersonationSession](#impersonationsession)
   - [Overview](#impersonationsession-overview)
   - [Methods](#impersonationsession-methods)
2. [ImpersonationExtensions](#impersonationextensions)
   - [Overview](#impersonationextensions-overview)
   - [Methods](#impersonationextensions-methods)
3. [Configuration](#configuration)

---

## ImpersonationSession

### ImpersonationSession Overview

The `ImpersonationSession` static class provides methods to manage user impersonation in an ASP.NET Core application. It allows an admin user to impersonate another user, end the impersonation, and display a widget indicating the impersonation status.

### ImpersonationSession Methods

- **`IsImpersonated()`**
  - Determines if the current user is in an impersonation session by checking for a specific role.
  - **Usage Example:**
    ```csharp
    bool isImpersonated = await ImpersonationSession.IsImpersonated();
    Console.WriteLine($"User is impersonated: {isImpersonated}");
    ```

- **`Impersonate(ILoginInfo user)`**
  - Initiates an impersonation session for the specified user, preserving the original user's claims as impersonator claims.
  - **Usage Example:**
    ```csharp
    var userToImpersonate = new GenericLoginInfo
    {
        DisplayName = "John Doe",
        ID = "123",
        Email = "john.doe@example.com",
        Roles = new[] { "User" }
    };
    await ImpersonationSession.Impersonate(userToImpersonate);
    Console.WriteLine("Impersonation started");
    ```

- **`EndImpersonation()`**
  - Ends the current impersonation session and restores the original user's identity.
  - **Usage Example:**
    ```csharp
    if (await ImpersonationSession.IsImpersonated())
    {
        await ImpersonationSession.EndImpersonation();
        Console.WriteLine("Impersonation ended");
    }
    ```

- **`GetWidget()`**
  - Returns an HTML snippet indicating the impersonation status if the user is impersonated.
  - **Usage Example:**
    ```csharp
    var widget = await ImpersonationSession.GetWidget();
    Console.WriteLine(widget); // Outputs: "<div class='impersonation-note'>Impersonating <b>John Doe</b></div>" or empty string
    ```

---

## ImpersonationExtensions

### ImpersonationExtensions Overview

The `ImpersonationExtensions` static class provides extension methods for working with `ClaimsPrincipal` objects during impersonation, including converting claims to and from impersonator format and retrieving the impersonator's name.

### ImpersonationExtensions Methods

- **`GetImpersonatorName(this ClaimsPrincipal @this)`**
  - Retrieves the name of the original user (impersonator) from the claims if the current user is impersonated.
  - **Usage Example:**
    ```csharp
    var user = Context.Current.Http().User; // Assuming an impersonated user
    var impersonatorName = user.GetImpersonatorName();
    Console.WriteLine($"Impersonator: {impersonatorName}");
    ```

---

## Configuration

The `ImpersonationSession` and `ImpersonationExtensions` classes do not require specific configuration settings from an `appsettings.json` file or environment variables beyond the standard Olive framework setup. They rely on the following prerequisites:

### Prerequisites
  - **Olive Framework Setup:**
  - The authentication system must be configured to support `SignInAsync` and `SignOutAsync` methods (typically via ASP.NET Core authentication middleware).

### Notes
 - The impersonation role (`Olive-IMPERSONATOR`) is hardcoded and automatically added to the impersonated user's roles.