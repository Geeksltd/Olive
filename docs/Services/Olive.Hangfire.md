# Olive.Hangfire

## Overview

The Olive Scheduled Tasks library allows Olive applications to easily integrate scheduled-background tasks execution using **Hangfire**, specifically configured to leverage SQL Server storage for reliable, distributed task scheduling. Additionally, it provides developer utilities for debugging and running recurring tasks manually.

---

## Table of contents

- [Features](#key-features)
- [Getting Started](#getting-started)
- [Core Classes and Methods](#core-classes-and-methods)
- [Configuration](#configuration) 
- [Dependencies](#dependencies)
- [Logging and Exception Handling](#logging-and-exception-handling)
- [Notes](#important-notes)

---

## Key Features

- Quickly integrate scheduled tasks in ASP.NET Core applications using Hangfire.
- Supports both SQL Server and MySQL storage providers via extension methods.
- Automatic task scheduling configuration via managed cron expressions.
- Developer-friendly tasks dashboard integration (Hangfire's built-in dashboard).
- Provides developer commands for debugging task executions directly from the browser UI.
- Seamless integration with Olive's Data infrastructure.

---

## Getting Started

Install the required NuGet packages:

```powershell
Install-Package Hangfire
Install-Package Hangfire.SqlServer
Install-Package Olive
Install-Package Olive.Entities
```

---

## Core Classes and Methods

### `SqlServerStorage` and Middleware Initialization

The Olive solution integrates Hangfire using SQL Server as the storage backend for scheduled tasks.  

### Extensions for .NET Core ease-of-use (`AddScheduledTasks`)

- Extension methods simplify configuring Hangfire with Olive integration in the ServiceCollection: 

```csharp
services.AddScheduledTasks();
```  

---

## Configuration (`appsettings.json`):

```json
{
  "ConnectionStrings": {
    "Default": "Server=myServer;Database=myDB;Trusted_Connection=True;"
  },
  "Automated.Tasks": {
    "Enabled": true
  }
}
```

---

## Developer Tools: `ScheduledTasksDevCommand`

Olive offers developer utilities to selectively run scheduled tasks manually, useful in development and debugging scenarios:

- Easy manual invocation through provided developer commands.
- Visual aids to see tasks clearly in a browser as HTML table views.

Example Developer Command usage via browser:

```
http://yourwebsite-domain/cmd/scheduled-tasks?run=DailyCleanupJob
```

---

## Dependencies

You must install and configure these packages and libraries:

- **Hangfire**: Scheduling engine.
- **Hangfire.SqlServer**: Storage provider for Microsoft SQL Server.
- **Olive Framework**: Provides integrated Context and Database access.

Install via NuGet:

```powershell
Install-Package Hangfire
Install-Package Hangfire.SqlServer
Install-Package Olive
Install-Package Olive.Entities
```

---

## Logging and Exception Handling

- Comprehensive logging for scheduled task executions and failures.
- Robust exceptions clearly identifying runtime errors or misconfigurations, e.g., missing database configuration.

Example Exception message:

```
Exception: Failed to open a DB connection. ConnectionString was invalid or database unreachable.
``` 

---

## Important Notes

- Ensure reliable database connectivity and permissions since job execution history and scheduling is stored in SQL Server.
- Consider securing your Hangfire dashboard for production deployments when Environments aren't development or debugging mode.
- Validate cron expressions and scheduled task execution carefully before enabling automated tasks in production.