# Olive.Mvc.lpFilter

## Overview
The **Olive.Mvc.lpFilter** component provides robust IP-based access control for web applications built on the Olive Framework. It allows you to restrict or permit access to your application based on IP addresses, IP ranges, countries, and geographical regions, implementing strong security measures to control access precisely according to your business requirements.

---

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Installation](#installation)
- [How IP Filter Works](#how-ip-filter-works)
- [Core Functionalities](#core-functionalities)
  - [Global IP Policy](#global-ip-policy)
  - [Country-Level Policy](#country-level-policy)
  - [Region-Level Policy](#region-level-policy)
  - [Specific IP Address Policies](#specific-ip-address-policies)
- [Usage Examples](#usage-examples)
- [Integration with ASP.NET Core](#integration-with-aspnet-core)
- [Country and Region Codes](#country-and-region-codes)
- [Exception Handling](#exception-handling)
- [Dependencies](#dependencies)
- [Security Information](#security-information)
- [Important Notes](#important-notes)

---

## Key Features

- **Flexible Policies**: Block or allow access based on global settings, specific IP addresses, country codes, or entire regions.
- **Country & Region Blocking**: Utilizes an external DB-IP CSV database to block IP ranges of specific regions or countries.
- **Circuit-breaker-like Resilience**: Ensures IP block lists are loaded only once and cached to reduce overhead.
- **Highly Customizable**: Easily customizable blocked-response pages.
- **Easy Integration with ASP.NET Core Middleware**: Simple hooks to apply IP filtering across your request pipeline.

---

## Installation

Add dependencies to your Olive project by installing these required packages:

```powershell
Install-Package Olive
Install-Package Olive.Mvc
```

### IP Database Setup

The IP filter relies on an external CSV database (DB-IP CSV database) to map IP addresses to countries. Download and put the file named **dbip-country.csv** from [DB-IP](https://db-ip.com/db/download/country) into the following directory under your Olive web application root:

```
YOUR_PROJECT_ROOT/wwwroot/--IPFilter/dbip-country.csv
```

---

## How IP Filter Works

- Incoming Request → Extract client's IP address → Check against global rules, country-based rules, region-based rules, and specific IP rules → Allow or disallow access.
- Uses a powerful IP address storage mechanism (`BigInteger`) ensuring fast and reliable range checks for IP address security.

---

## Core Functionalities

### Global IP Policy
Set a global allowance or disallowance policy for all incoming connections:

```csharp
// Allow all IPs globally (default):
IpFilter.SetGlobalPolicy(IpFilter.Policy.Allow); 

// or Disallow all IPs globally:
IpFilter.SetGlobalPolicy(IpFilter.Policy.Disallow);
```

### Country-Level Policy 
Block or allow IP addresses from specific countries:
```csharp
// Disallow access from specific countries:
IpFilter.SetCountryPolicy(IpFilter.Policy.Disallow, "CN", "RU", "IR");

// Allow access for specific countries overriding previous block (countries are identified by 2-letter ISO code):
IpFilter.SetCountryPolicy(IpFilter.Policy.Allow, "GB", "US");
```

### Region-Level Policy
Easily restrict entire global regions with region codes:

```csharp
// Disallow IPs from entire regions:
IpFilter.SetRegionPolicy(IpFilter.Policy.Disallow, IpFilter.Region.Europe, IpFilter.Region.Asia);

// or Allow from entire regions:
IpFilter.SetRegionPolicy(IpFilter.Policy.Allow, IpFilter.Region.NorthAmerica);
```

### Specific IP Address Policies
Specify IP addresses explicitly, overriding all references from regional or country-based rules:

```csharp
// Specifically allow certain IPs:
IpFilter.SetSpecificIpPolicy(IpFilter.Policy.Allow, "8.8.8.8");

// or specifically disallow certain IPs:
IpFilter.SetSpecificIpPolicy(IpFilter.Policy.Disallow, "1.2.3.4");
```

---

## Usage Examples

### Integration Example in ASP.NET Core pipeline (`Startup.cs`)
Easily integrate IP filtering into your `Configure` method:

```csharp
app.Use(async (context, next) =>
{
    await IpFilter.BlockIfNecessary(context); // Check if current request should be blocked.
    await next();
});
```

### Set a blocking message or redirect page
Customize the response shown to users upon blocked requests as follows:

```csharp
IpFilter.BlockedAttemptResponse = "Access Restricted: Your region is restricted.";
```

### Customize Blocked Actions
Invoke a custom action instead of default message return:

```csharp
IpFilter.OnBlockedAccessAttempt = async () =>
{
    // Log additional information, notify admins, redirect etc.
    await Context.Current.Response().Redirect("/blockedregion");
};
```

---

## Integration with ASP.NET Core 
You can simply integrate this into your middleware pipeline:

```csharp
public void Configure(IApplicationBuilder app)
{
    app.Use(async (context, next) =>
    {
        await IpFilter.BlockIfNecessary(context);
        await next.Invoke();
    });
}
```

---

## Country and Region Codes
The IP Filter uses standard ISO two-letter-code abbreviations for country restriction and predefined region codes:

| Region | Code |
|--------|------|
| Africa | AF |
| Antarctica | AN |
| Asia | AS |
| Europe | EU |
| North America | NA |
| Oceania | OC |
| South America | SA |

---

## Exception Handling

Provides clear exceptions on IP-address parsing failures or if required configurations are missing:

- Missing CSV DB-IP database file will throw exception:

  ```
  Could not find the file 'dbip-country.csv'.
  ```

- Invalid IP address parsing errors provided clearly:

  ```
  Cannot convert the specified IP address string of 'INVALID-IP' to unit IP address value.
  ```

---

## Dependencies
To use Olive IP Filter, integrate these packages:

- Olive Framework (`Install-Package Olive`)
- Entities Management (`Install-Package Olive.Entities`)
- Olive.Mvc (`Install-Package Olive.Mvc`)
- CSV handling via Olive.Csv (`Install-Package Olive.Csv`)

Ensure dependencies are added consecutively in your Olive web application setup, and these Nuget packages are included:

```powershell
Install-Package Olive
Install-Package Olive.Mvc
Install-Package Olive.Csv
```

---

## Security Information

Ensure your IP Filtering settings themselves are protected carefully. Any privileged exceptions (such as office IP addresses) or policy configurations may have security implications if improperly exposed or accidentally misconfigured.

Always ensure logging of any allowed or blocked attempts is meaningful and monitored:

```csharp
IpFilter.OnBlockedAccessAttempt = async () =>
{
    await Context.Current.Response().EndWith("Access blocked due to policy restrictions.");

    // additional logging here
};
```

---

## Important Notes

- Carefully test all your IP filtering rules in staging before deploying to production environments. Misconfigured rules can lock out valid users.
- Ensure your DB-IP CSV file is frequently updated from reliable IP tagging services (DB-IP is recommended).
- When accessing client IP addresses, ensure you handle trusted proxies correctly with forwarded headers middleware to maintain reliable IP checking.
- Periodically review the IP filtering list, policies, and log entries to adapt to legitimate business and operational changes.