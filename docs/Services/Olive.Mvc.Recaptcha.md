# Olive.Mvc.Recaptcha

## Overview

The Olive reCAPTCHA module adds Google reCAPTCHA services to Olive MVC-based ASP.NET Core web applications. It provides server-side validation of user-submitted reCAPTCHA responses, ensuring robust protection for web forms against bots and spam. The component supports seamless integration of reCAPTCHA in both client-side and server-side contexts, aligns with ASP.NET Core MVC filters, and ensures secure access with clear localization and configuration capabilities.

---

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Configuration](#configuration)
- [Key Components](#key-components)
- [Exception Handling](#exception-handling-and-logging)
- [Dependencies](#dependencies)
- [Important Notes](#important-notes)

---

## Features

- **Google reCAPTCHA Integration**: Quickly integrate Google reCAPTCHA with your Olive MVC application.
- **Server-side Validation**: Securely handles validation of reCAPTCHA responses against Google's verification endpoint.
- **Localization Support**: Automatically aligns with application locale settings.
- **MVC Filters Integration**: Easily utilize MVC authorization filter pipeline for automatic validation.
- **Configurable and Extensible**: Simple configuration through appsettings.json or programmatic initialization, suitable for extensive customizations.
- **Comprehensive Error Handling and Logging**: Provides structured errors and logging about reCAPTCHA processes for improved monitoring and diagnostics.

---

## Installation

Use NuGet Package Manager to add the Olive reCAPTCHA integration:

```powershell
Install-Package Olive.Mvc
```

Ensure you also prefer the standard ASP.NET Core dependencies:

- `Microsoft.AspNetCore.Mvc`
- `Microsoft.Extensions.Logging`
- `Newtonsoft.Json`

---

## Configuration

Configure your `appsettings.json` with Google’s provided keys:

```json
{
  "Recaptcha": {
    "SiteKey": "[YOUR_SITE_KEY_FROM_GOOGLE]",
    "SecretKey": "[YOUR_SECRET_KEY_FROM_GOOGLE]"
  }
}
```

Or configure programmatically in your `Startup.cs`:

```csharp
services.AddRecaptcha(options =>
{
    options.SiteKey = "[YOUR_SITE_KEY_FROM_GOOGLE]";
    options.SecretKey = "[YOUR_SECRET_KEY_FROM_GOOGLE]";
    options.Enabled = true;
});
```

- Import the tag helpers from **_ViewImport.cshtml** like `@addTagHelper *, Olive.Mvc.Recaptcha`.
- Add the javascript tag in your layout like:
```html
@if (!Request.IsAjaxRequest())
{
    <script src="/lib/requirejs/require.js" data-main="/scripts/references.js?v=1"></script>
    <recaptcha-script />
}
```
- Add captcha to your module with somethig like `CustomField().ControlMarkup("<recaptcha />");`.

---

## Key Components

### RecaptchaService Class

Manages the validation logic against Google reCAPTCHA API, ensuring secure, robust, and reliable validation for submitted reCAPTCHA tokens.

Key methods:

- `Task ValidateResponseAsync(string response, string remoteIp)` - verifies the reCAPTCHA response.
- Provides structured exception handling detailing the verification outcomes.

### RecaptchaOptions Class

Holds configuration options necessary for the Olive reCAPTCHA operation.

| Property             | Description                                |
|----------------------|--------------------------------------------|
| `SiteKey`            | Public site key given by Google             |
| `SecretKey`          | Secret key for validation at your server    |
| `Enabled`            | Enables or disables reCAPTCHA validation    |
| `JavaScriptUrl`      | URL for reCAPTCHA script                    |
| `ValidationMessage`  | Custom message displayed when validation fails|
| `LanguageCode`       | Sets specific reCAPTCHA locale              |

### ValidateRecaptchaAttribute Attribute

A MVC action attribute to protect controller actions with reCAPTCHA validation seamlessly.

```csharp
[ValidateRecaptcha]
public IActionResult SubmitForm(MyViewModel model)
{
   // Your action logic here
}
```

Or validate you action with an attribute:
```csharp
Button("Register").IsDefault()
    .ExtraActionMethodAttributes("[ValidateRecaptcha]");
``` 

### RecaptchaValidationException Class

Encapsulates structured error information for reCAPTCHA validation errors and code:

```csharp
public class RecaptchaValidationException : Exception
{
    public bool InvalidResponse { get; private set; }
}
```

--- 

## Exception Handling and Logging

The logger captures informative exceptions on validation failures:

- Provides clear logs identifying what went wrong during Google reCAPTCHA requests.
- Reports errors to assist debugging and configuration corrections when necessary.
- Example logged validation exception:

```
Exception: "Looks up a localized string similar to Recaptcha validation failed. The response parameter is missing."
```

---

## Dependencies

Include these in your Olive and ASP.NET Core MVC projects:

- ASP.NET Core MVC (`Microsoft.AspNetCore.Mvc`)
- Microsoft.Extensions.Logging
- Newtonsoft.Json (for JSON serialization)

Use NuGet to ensure they're properly included:

```powershell
Install-Package Olive.Mvc
Install-Package Microsoft.Extensions.Logging
Install-Package Newtonsoft.Json
```

---

## Important Notes

- Always secure your configuration keys; avoid exposing them publicly.
- Customize reCAPTCHA appearance to match site design through provided options.
- Localization support is direct, specify language via `LanguageCode` or accept automatic client culture assignment (default behavior).
- Consider network latency when adjusting the BackchannelTimeout to avoid unnecessary exception handling due to premature timeouts.