# Olive.Web

This document provides an overview and usage examples for the public classes and methods in the `Olive`, `Olive.Web`, and `Olive.Mvc` namespaces. These extensions enhance web-related functionality in ASP.NET Core applications, including API authentication, HTTP request/response handling, cookie management, caching, and ETag support.

---

## Table of Contents

1. [OliveWebExtensions](#olivewebextensions)
   - [Overview](#olivewebextensions-overview)
   - [Methods](#olivewebextensions-methods)
2. [CookieProperty](#cookieproperty)
   - [Overview](#cookieproperty-overview)
   - [Methods](#cookieproperty-methods)
3. [HttpContextCache](#httpcontextcache)
   - [Overview](#httpcontextcache-overview)
   - [Methods](#httpcontextcache-methods)
4. [SupportETagCacheAttribute](#supportetagcacheattribute)
   - [Overview](#supportetagcacheattribute-overview)
   - [Methods](#supportetagcacheattribute-methods)

---

## OliveWebExtensions

### OliveWebExtensions Overview

The `OliveWebExtensions` static class provides a collection of extension methods for enhancing web functionality, including API client authentication, HTTP context access, request/response utilities, and more, within the Olive framework.

### OliveWebExtensions Methods

- **`AsHttpUser(this ApiClient @this)`**
  - Authenticates an `ApiClient` instance with the current user's cookies.
  - **Usage Example:**
    ```csharp
    var client = new ApiClient("https://api.example.com");
    client.AsHttpUser();
    var response = await client.Get<string>("endpoint");
    ```

- **`User(this Context context)`**
  - Retrieves the current user's `ClaimsPrincipal`.
  - **Usage Example:**
    ```csharp
    var user = Context.Current.User();
    Console.WriteLine(user?.Identity?.Name);
    ```

- **`Http(this Context context)`**
  - Retrieves the current `HttpContext`.
  - **Usage Example:**
    ```csharp
    var httpContext = Context.Current.Http();
    ```

- **`Request(this Context context)`**
  - Retrieves the current `HttpRequest`.
  - **Usage Example:**
    ```csharp
    var request = Context.Current.Request();
    ```

- **`GetCookies(this HttpRequest @this)`**
  - Retrieves all cookies from the request.
  - **Usage Example:**
    ```csharp
    var cookies = Context.Current.Request().GetCookies();
    foreach (var cookie in cookies) Console.WriteLine($"{cookie.Key}: {cookie.Value}");
    ```

- **`GetOrDefault<T>(this HttpRequest @this, string key)`**
  - Retrieves an entity or value from the request by key, returning default if not found.
  - **Usage Example:**
    ```csharp
    var user = await Context.Current.Request().GetOrDefault<User>("userId");
    ```

- **`Get<T>(this HttpRequest @this, string key)`**
  - Retrieves an entity or value from the request by key, throwing an exception if not found.
  - **Usage Example:**
    ```csharp
    var user = await Context.Current.Request().Get<User>("userId");
    ```

- **`GetList<T>(this HttpRequest @this, string key)`**
  - Retrieves a list of entities from the request by key, split by comma.
  - **Usage Example:**
    ```csharp
    var users = Context.Current.Request().GetList<User>("userIds");
    foreach (var userTask in users) Console.WriteLine((await userTask)?.ToString());
    ```

- **`FindGoogleSearchKeyword(this HttpRequest @this)`**
  - Extracts Google search keywords from the referrer URL.
  - **Usage Example:**
    ```csharp
    var keyword = Context.Current.Request().FindGoogleSearchKeyword();
    Console.WriteLine(keyword); // e.g., "olive framework"
    ```

- **`GetIPAddress(this HttpRequest @this)`**
  - Gets the client's IP address.
  - **Usage Example:**
    ```csharp
    var ip = Context.Current.Request().GetIPAddress();
    Console.WriteLine(ip);
    ```

- **`Has(this HttpRequest @this, string argument)`**
  - Checks if a parameter exists in the request.
  - **Usage Example:**
    ```csharp
    bool hasParam = Context.Current.Request().Has("key");
    ```

- **`RootUrl(this HttpRequest @this)`**
  - Gets the root URL of the request.
  - **Usage Example:**
    ```csharp
    var root = Context.Current.Request().RootUrl();
    Console.WriteLine(root); // e.g., "https://example.com/"
    ```

- **`ToAbsoluteUri(this HttpRequest @this)`**
  - Gets the absolute URI of the request.
  - **Usage Example:**
    ```csharp
    var uri = Context.Current.Request().ToAbsoluteUri();
    Console.WriteLine(uri);
    ```

- **`IsAjaxRequest(this HttpRequest @this)`**
  - Determines if the request is an AJAX call.
  - **Usage Example:**
    ```csharp
    bool isAjax = Context.Current.Request().IsAjaxRequest();
    ```

- **`RedirectPermanent(this HttpResponse response, string permanentUrl)`**
  - Issues a 301 redirect to a permanent URL.
  - **Usage Example:**
    ```csharp
    Context.Current.Response().RedirectPermanent("https://new-url.com");
    ```

- **`Dispatch(this HttpResponse response, FileInfo responseFile, string fileName = null, string contentType = "Application/octet-stream")`**
  - Sends a file to the client.
  - **Usage Example:**
    ```csharp
    var file = new FileInfo("path/to/file.pdf");
    await Context.Current.Response().Dispatch(file, "download.pdf");
    ```

- **`Dispatch(this HttpResponse response, byte[] responseData, string fileName, string contentType = "Application/octet-stream")`**
  - Sends binary data as a file to the client.
  - **Usage Example:**
    ```csharp
    var data = System.Text.Encoding.UTF8.GetBytes("Hello");
    await Context.Current.Response().Dispatch(data, "hello.txt");
    ```

- **`EndWith(this HttpResponse response, string message, string mimeType = "text/html")`**
  - Writes a message and ends the response.
  - **Usage Example:**
    ```csharp
    await Context.Current.Response().EndWith("Done!");
    ```

- **`Write(this HttpResponse response, string message)`**
  - Writes a message to the response synchronously.
  - **Usage Example:**
    ```csharp
    Context.Current.Response().Write("Hello");
    ```

---

## CookieProperty

### CookieProperty Overview

The `CookieProperty` static class provides methods to manage HTTP cookies, including getting and setting values in the request and response.

### CookieProperty Methods

- **`Get<T>(string propertyName = null, T defaultValue = default)`**
  - Retrieves a cookie value by key, with an optional default.
  - **Usage Example:**
    ```csharp
    var value = await CookieProperty.Get<string>("MyCookie", "default");
    Console.WriteLine(value);
    ```

- **`Set<T>(string propertyName = null, T value, bool isHttpOnly = true)`**
  - Sets a cookie value.
  - **Usage Example:**
    ```csharp
    CookieProperty.Set("MyCookie", "value");
    ```

- **`SetList<T>(string propertyName, IEnumerable<T> list, bool isHttpOnly = true)`**
  - Sets a list of entity IDs as a cookie.
  - **Usage Example:**
    ```csharp
    var users = new[] { new User { ID = "1" }, new User { ID = "2" } };
    CookieProperty.SetList("UserIds", users);
    ```

- **`GetList<T>(string propertyName = null)`**
  - Retrieves a list of entities from a cookie.
  - **Usage Example:**
    ```csharp
    var users = await CookieProperty.GetList<User>("UserIds");
    foreach (var user in users) Console.WriteLine(user?.ID);
    ```

- **`Remove(string propertyName)`**
  - Removes a cookie by key.
  - **Usage Example:**
    ```csharp
    CookieProperty.Remove("MyCookie");
    ```

---

## HttpContextCache

### HttpContextCache Overview

The `HttpContextCache` static class provides methods for caching objects within the scope of an HTTP request.

### HttpContextCache Methods

- **`GetOrAdd<TKey, TValue>(TKey key, Func<TValue> valueProducer)`**
  - Gets or adds a value to the HTTP context cache.
  - **Usage Example:**
    ```csharp
    var cachedValue = HttpContextCache.GetOrAdd("key", () => "computed value");
    Console.WriteLine(cachedValue);
    ```

- **`Remove<TKey>(TKey key)`**
  - Removes a value from the HTTP context cache.
  - **Usage Example:**
    ```csharp
    HttpContextCache.Remove("key");
    ```

---

## SupportETagCacheAttribute

### SupportETagCacheAttribute Overview

The `SupportETagCacheAttribute` class is an action filter that enables ETag-based caching for GET requests, returning a `304 Not Modified` response when the client’s ETag matches the server’s.

### SupportETagCacheAttribute Methods

- **`OnActionExecuted(ActionExecutedContext context)`**
  - Executes after the action, implementing ETag caching logic.
  - **Usage Example:**
    ```csharp
    [SupportETagCache]
    public IActionResult GetData()
    {
        return Ok("Data");
    }
    // Client request with matching ETag header returns 304
    ```