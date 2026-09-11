# Olive.Mvc.Security

This document provides an overview and usage examples for the public classes and methods in the `Olive.Security` namespace and related extensions in the Olive framework. It focuses on functionality available to users, such as data protection, authentication, and JWT token management in an ASP.NET Core environment.

---

## Table of Contents

1. [SymmetricKeyDataProtector](#symmetrickeydataprotector)
   - [Overview](#symmetrickeydataprotector-overview)
   - [Methods](#symmetrickeydataprotector-methods)
2. [OliveSecurityExtensions](#olivesecurityextensions)
   - [Overview](#olivesecurityextensions-overview)
   - [Methods](#olivesecurityextensions-methods)
3. [OAuth](#oauth)
   - [Overview](#oauth-overview)
   - [Methods](#oauth-methods)
4. [JwtAuthenticateAttribute](#jwtauthenticateattribute)
   - [Overview](#jwtauthenticateattribute-overview)
   - [Methods](#jwtauthenticateattribute-methods)
5. [ILoginInfo](#ilogininfo)
   - [Overview](#ilogininfo-overview)
6. [GenericLoginInfo](#genericlogininfo)
   - [Overview](#genericlogininfo-overview)
7. [ExternalLoginInfo](#externallogininfo)
   - [Overview](#externallogininfo-overview)

---

## SymmetricKeyDataProtector

### SymmetricKeyDataProtector Overview

The `SymmetricKeyDataProtector` class implements `IDataProtector` to provide symmetric key-based encryption and decryption with GZip compression.

### SymmetricKeyDataProtector Methods

- **`CreateProtector(string purpose)`**
  - Creates a new protector instance with a specific purpose.
  - **Usage Example:**
    ```csharp
    var protector = new SymmetricKeyDataProtector("my-secret-key");
    var purposeProtector = protector.CreateProtector("user-data");
    ```

- **`Protect(byte[] plaintext)`**
  - Encrypts and compresses the provided data.
  - **Usage Example:**
    ```csharp
    var protector = new SymmetricKeyDataProtector("my-secret-key");
    byte[] data = System.Text.Encoding.UTF8.GetBytes("Sensitive info");
    byte[] encrypted = protector.Protect(data);
    ```

- **`Unprotect(byte[] protectedData)`**
  - Decompresses and decrypts the provided data.
  - **Usage Example:**
    ```csharp
    var protector = new SymmetricKeyDataProtector("my-secret-key");
    byte[] encrypted = protector.Protect(System.Text.Encoding.UTF8.GetBytes("Sensitive info"));
    byte[] decrypted = protector.Unprotect(encrypted);
    Console.WriteLine(System.Text.Encoding.UTF8.GetString(decrypted)); // Outputs: "Sensitive info"
    ```

---

## OliveSecurityExtensions

### OliveSecurityExtensions Overview

The `OliveSecurityExtensions` static class offers extension methods for authentication, JWT token creation, and user management.

### OliveSecurityExtensions Methods

- **`ToClaimsIdentity(this ILoginInfo @this)`**
  - Converts an `ILoginInfo` to a `ClaimsIdentity`.
  - **Usage Example:**
    ```csharp
    ILoginInfo user = new GenericLoginInfo { DisplayName = "John Doe", ID = "123" };
    var identity = user.ToClaimsIdentity();
    ```

- **`CreateJwtToken(this ILoginInfo @this, IEnumerable<Claim> additionalClaims = null, bool remember = false)`**
  - Generates a JWT token for the user.
  - **Usage Example:**
    ```csharp
    ILoginInfo user = new GenericLoginInfo { DisplayName = "John Doe", ID = "123" };
    var extraClaims = new List<Claim> { new Claim("Custom", "Value") };
    string token = user.CreateJwtToken(extraClaims, remember: true);
    Console.WriteLine(token);
    ```

- **`LogOn(this ILoginInfo @this, IEnumerable<Claim> additionalClaims = null, bool remember = false)`**
  - Signs in a user asynchronously.
  - **Usage Example:**
    ```csharp
    ILoginInfo user = new GenericLoginInfo { DisplayName = "John Doe", ID = "123" };
    await user.LogOn(remember: true);
    ```

- **`IsPersistent(this ClaimsPrincipal @this)`**
  - Checks if the user’s session is persistent.
  - **Usage Example:**
    ```csharp
    var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.IsPersistent, "true") }, "Olive"));
    bool isPersistent = principal.IsPersistent(); // Returns true
    ```

- **`GetExpiration(this ClaimsPrincipal @this)`**
  - Gets the session expiration time.
  - **Usage Example:**
    ```csharp
    var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Expiration, DateTimeOffset.UtcNow.AddDays(1).ToString()) }, "Olive"));
    DateTimeOffset expiration = principal.GetExpiration();
    Console.WriteLine(expiration);
    ```

- **`Is(this ILoginInfo @this, ClaimsPrincipal loggedInUser)`**
  - Checks if the `ILoginInfo` ID matches the logged-in user’s ID.
  - **Usage Example:**
    ```csharp
    ILoginInfo user = new GenericLoginInfo { ID = "123" };
    var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "123") }, "Olive"));
    bool isMatch = user.Is(principal); // Returns true
    ```

- **`Is(this ClaimsPrincipal @this, ILoginInfo loginInfo)`**
  - Checks if the logged-in user’s ID matches the `ILoginInfo` ID.
  - **Usage Example:**
    ```csharp
    var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "123") }, "Olive"));
    ILoginInfo user = new GenericLoginInfo { ID = "123" };
    bool isMatch = principal.Is(user); // Returns true
    ```

- **`Clone(this ILoginInfo @this, Action<GenericLoginInfo> change = null)`**
  - Clones an `ILoginInfo` with optional modifications.
  - **Usage Example:**
    ```csharp
    ILoginInfo user = new GenericLoginInfo { DisplayName = "John", ID = "123" };
    var clone = user.Clone(c => c.DisplayName = "Jane");
    Console.WriteLine(clone.DisplayName); // Outputs: "Jane"
    ```

---

## OAuth

### OAuth Overview

The `OAuth` class manages external logins, JWT decoding, and session operations.

### OAuth Methods

- **`LogOff()`**
  - Signs out the current user and clears the session.
  - **Usage Example:**
    ```csharp
    await OAuth.Instance.LogOff();
    ```

- **`LoginBy(string provider)`**
  - Initiates an external login with a provider.
  - **Usage Example:**
    ```csharp
    await OAuth.Instance.LoginBy("Google");
    ```

- **`NotifyExternalLoginAuthenticated(ExternalLoginInfo info)`**
  - Notifies event subscribers of an external login.
  - **Usage Example:**
    ```csharp
    OAuth.Instance.ExternalLoginAuthenticated += async (info) => Console.WriteLine(info.Args.Email);
    var info = new ExternalLoginInfo { Email = "user@example.com", IsAuthenticated = true };
    await OAuth.Instance.NotifyExternalLoginAuthenticated(info);
    ```

- **`DecodeJwt(string jwt)`**
  - Decodes a JWT token into a `ClaimsPrincipal`.
  - **Usage Example:**
    ```csharp
    string jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."; // Valid JWT token
    var principal = OAuth.Instance.DecodeJwt(jwt);
    if (principal != null) Console.WriteLine(principal.Identity.Name);
    ```

---

## JwtAuthenticateAttribute

### JwtAuthenticateAttribute Overview

The `JwtAuthenticateAttribute` is an action filter for authenticating requests via JWT tokens in the `Authorization` header.

### JwtAuthenticateAttribute Methods

- **`OnActionExecuting(ActionExecutingContext context)`**
  - Authenticates the request using a JWT token before action execution.
  - **Usage Example:**
    ```csharp
    [JwtAuthenticate]
    public IActionResult SecureAction()
    {
        return Ok("Authenticated!");
    }
    // Add "Authorization: Bearer <token>" header to the request
    ```

---

## ILoginInfo

### ILoginInfo Overview

The `ILoginInfo` interface defines the structure for user login information. Users typically implement this for custom login logic.

- **Usage Example:**
  ```csharp
  public class CustomUser : ILoginInfo
  {
      public string DisplayName => "Alice";
      public string ID => "456";
      public string Email => "alice@example.com";
      public TimeSpan? Timeout => TimeSpan.FromHours(1);
      public IEnumerable<string> GetRoles() => new[] { "User" };
  }
  ```

---

## GenericLoginInfo

### GenericLoginInfo Overview

The `GenericLoginInfo` class is a concrete implementation of `ILoginInfo` for general use.

- **Usage Example:**
  ```csharp
  var user = new GenericLoginInfo
  {
      DisplayName = "Bob",
      ID = "789",
      Email = "bob@example.com",
      Roles = new[] { "Admin" },
      Timeout = TimeSpan.FromMinutes(30)
  };
  ```

---

## ExternalLoginInfo

### ExternalLoginInfo Overview

The `ExternalLoginInfo` class holds data about an external login attempt.

- **Usage Example:**
  ```csharp
  var loginInfo = new ExternalLoginInfo
  {
      IsAuthenticated = true,
      Issuer = "Google",
      Email = "user@gmail.com",
      NameIdentifier = "google-123",
      UserName = "User"
  };
  ```