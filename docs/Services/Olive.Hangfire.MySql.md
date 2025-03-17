# Olive.Hangfire.MySql - Hangfire Integration (MySQL)

## Overview

This Olive.Hangfire.MySql integrates Olive applications with Hangfire to provide scheduled background job processing. It enables your Olive application to easily leverage recurring task execution using Hangfire's powerful scheduling capabilities backed specifically by MySQL storage. It simplifies the configuration and registration of scheduled tasks, background job processing, and monitoring via Hangfire dashboard.

---

## Table of Contents

- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Core Extension Methods](#core-extension-methods)
  - [`AddScheduledTasks`](#addscheduledtasks)
- [Monitoring Jobs with Dashboard](#enabling-hangfire-dashboard)
- [Exception Handling](#exception-handling)
- [Dependencies](#dependencies)
- [Important Notes](#important-notes)

---

## Getting Started

To use Olive Scheduled Tasks with MySQL through Hangfire, install the required NuGet packages:

```powershell
Install-Package Hangfire
Install-Package Hangfire.MySql
```

Then register the scheduled tasks in your application's `Startup.cs` file.

**Step 1: Configure in `Startup.cs` (inside `ConfigureServices`)**

```csharp 
services.AddScheduledTasks(); // Adds basic scheduled tasks capability. 
``` 

---

## Configuration

Specify your connection settings within `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "server=myserver;database=mydatabase;uid=user;pwd=password;Allow User Variables=True;"
  }
}
```

Ensure this default connection string points to your MySQL database, where Hangfire data will be stored.

---

## Core Functionalities

- **Easy Setup:** Quickly configure scheduled tasks and Hangfire integration for MySQL.
- **Integrated Dashboard:** Automatically starts Hangfire dashboard when debugging, granting visual monitoring of jobs.
- **Recurring Task Registration:** Use BackgroundJobsPlans to clearly define your periodic tasks.

---

## Core Methods

### AddScheduledTasks (IServiceCollection Extension)

Registers Hangfire and initializes it with MySQL-backed storage using Olive's current database connection string.

```csharp
services.AddScheduledTasks();
``` 

---

### UseScheduledTasks\<TPlan> (IApplicationBuilder Extension)

Initializes Hangfire server to execute the scheduled tasks defined in your project. When debugging, automatically starts Hangfire dashboard on a convenient URL.

**Example Usage:**

```csharp
app.UseScheduledTasks<MyRecurringTaskPlan>();
```

Here `BackgroundJobsPlan` is a user-defined plan class listing recurrent tasks.

---
 
## Integration Details

The Olive integration configures Hangfire to use MySQL as its job storage backend, specifically utilizing Hangfire.MySql. This is optimized for applications already using MySQL as their primary database, providing seamless integration and reduced setup complexity.

---

## Exception Handling

Meaningful exceptions and errors are managed by the Olive integration:

- Database connection issues are raised clearly during startup:
```
Exception: "Failed to open a DB connection."
```

- Misconfigured or invalid cron expressions also raise exceptions.

Additional logs and exceptions related to missing configurations or connection issues clearly describe failing situations:

```
"Could not connect to the provided MySQL database."
```

---

## Dependencies

Ensure the following NuGet packages are installed and configured:

```powershell
Install-Package Hangfire
Install-Package Hangfire.MySql
Install-Package Olive
Install-Package Olive.Entities
```

Ensure `MySql.Data` or `MySqlConnector` is also installed for MySQL connectivity. 

---

## Important Notes

- Verify application permissions for creating and managing MySQL schema needed by Hangfire.
- Ensure that production systems are secured, and restrict Hangfire dashboard access appropriately.
- Hangfire's MySQL storage requires a properly configured database, including correct character encoding support (utf8mb4_unicode_ci recommended) to avoid issues such as truncation.