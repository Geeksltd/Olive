# Olive.PassiveBackgroundTasks

This document provides an overview and usage examples for the public classes and methods in the `Olive.PassiveBackgroundTasks` namespace. It enables scheduling and execution of background tasks in an ASP.NET Core application using a cron-based approach. A configuration section details the required settings in an `appsettings.json` file.

---

## Table of Contents

1. [DistributedBackgroundTasksMiddleware](#distributedbackgroundtasksmiddleware)
   - [Overview](#distributedbackgroundtasksmiddleware-overview)
   - [Methods](#distributedbackgroundtasksmiddleware-methods)
2. [Extensions](#extensions)
   - [Overview](#extensions-overview)
   - [Methods](#extensions-methods)
3. [IBackgourndTask](#ibackgourndtask)
   - [Overview](#ibackgourndtask-overview)
4. [Configuration](#configuration)

---

## DistributedBackgroundTasksMiddleware

### DistributedBackgroundTasksMiddleware Overview

The `DistributedBackgroundTasksMiddleware` class is an ASP.NET Core middleware that triggers the execution of scheduled background tasks when a specific HTTP request is received. It supports forced execution and task-specific runs via query parameters.

### DistributedBackgroundTasksMiddleware Methods

- **`Invoke(HttpContext httpContext)`**
  - Handles HTTP requests to trigger background task execution and returns a report as an HTML response.
  - **Usage Example:**
    ```csharp
    // Trigger all tasks: GET /olive-trigger-tasks
    // Force all tasks: GET /olive-trigger-tasks?force=true
    // Run specific task: GET /olive-trigger-tasks?key=MyTask
    // The middleware will respond with a list of messages, e.g., "Selected tasks: MyTask", "Finished running MyTask"
    ```

---

## Extensions

### Extensions Overview

The `Extensions` static class provides extension methods to integrate scheduled background tasks into an ASP.NET Core application's service collection and middleware pipeline.

### Extensions Methods

- **`AddScheduledTasks<TPlan, TBackgroundTask>(this IServiceCollection services)`**
  - Registers a background task type and its plan in the dependency injection container.
  - **Usage Example:**
    ```csharp     
    services.AddScheduledTasks();
    ``` 
  
---

## IBackgourndTask

### IBackgourndTask Overview

The `IBackgourndTask` interface defines the contract for a background task entity, which is persisted and managed by the `Engine`. Users typically implement this interface for custom task definitions.

- **Usage Example:**
```csharp
using MSharp;

namespace Domain
{
    class BackgroundTask : EntityType
    {
        public BackgroundTask()
        {
            Implements("Olive.PassiveBackgroundTasks.IBackgourndTask");
            String("Name").Mandatory().Unique();
            Guid("Executing instance");
            DateTime("Heartbeat");
            DateTime("Last executed");
            Int("Interval in minutes").Mandatory();
            Int("Timeout in minutes").Mandatory();
        }
    }
} 
```

---

## Configuration

The `Olive.PassiveBackgroundTasks` library relies on a specific configuration setting stored in an `appsettings.json` file with a JSON structure. Below is the required configuration:

- **`Automated.Tasks:Enabled`** (Required)
  - A boolean flag to enable or disable the scheduled tasks feature. If set to `false`, the middleware and task registration will be skipped.
  - **Example:**
    ```json
    {
      "Automated": {
        "Tasks": {
          "Enabled": true
        }
      }
    }
    ``` 

### Notes
- If `Automated.Tasks:Enabled` is not set or is `false`, the `UseScheduledTasks` method will bypass task registration and middleware setup. 
- The cron expressions used in `BackgroundJobsPlan` (e.g., `"0 * * * *"` for hourly) determine task intervals, parsed by `CronParser`.