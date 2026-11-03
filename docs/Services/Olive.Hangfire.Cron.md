# Olive.Hangfire.Cron (Hangfire Integration)

## Overview

The Olive.Hangfire.Cron includes convenient scheduling utilities leveraging the standard cron expression syntax via the `Cron` class, inspired by the Hangfire task scheduling library. This class provides simple methods to generate cron expressions which represent common recurring time intervals, simplifying scheduler configurations for background tasks in applications.

---

## Table of Contents

- [Overview](#overview)
- [Cron Class Functionalities](#cron-class-functionalities)
- [Supported Scheduling Methods](#supported-scheduling-methods)
- [Method Usage Examples](#method-usage-examples)
- [Exception Handling](#exception-handling)

---

## Cron Class Functionalities

The `Cron` helper class contains predefined templates and methods for generating cron expressions compatible with the popular Hangfire job scheduler. It includes methods for creating cron expressions easily for common time intervals such as hourly, daily, weekly, monthly, and yearly.

Each method returns a cron string in the well-known "* * * * *" syntax, indicating the exact interval or frequency for scheduled tasks.

---

## Supported Scheduling Methods

The available methods in this utility class are:

| Method             | Description                         | Example cron format |
| ------------------- | -----------------------------------|-----------------------|
| `Never()` | Returns an expression that never fires (31st February). | `"0 0 31 2 *"` |
| `Hourly(int minute)` | Fires every hour at the specified minute |
| `Daily(int hour, int minute)` | Fires every day at a specified hour and minute |
| `Weekly(DayOfWeek dayOfWeek, int hour, int minute)` | Fires weekly at the given day and time |
| `Monthly(int day, int hour, int minute)` | Fires monthly at the provided day and time |
| `Yearly(int month, int day, int hour, int minute)` | Fires annually at the specific month, day, and time |
| `MinuteInterval(int interval)` | Fires every "specified interval" minutes |
| `HourInterval(int interval)` | Fires every specified number of hours |
| `MonthInterval(int interval)` | Fires every specified number of months |
| `Never()` | Cron expression that never fires |

---

## Method Usage Examples

### Scheduling tasks hourly:
```csharp
// Schedule at minute 15 of every hour
var hourlyCron = Cron.Hourly(15);
// hourlyCron value is: "15 * * * *"
```

### Scheduling tasks daily:
```csharp
// Every day at 9:30 AM
var dailyCron = Cron.Daily(9, 30);
```

### Scheduling tasks weekly:
```csharp
// Every Friday at 18:00
var weeklyCron = Cron.Weekly(DayOfWeek.Friday, 18, 0);
```

### Scheduling tasks monthly:
```csharp
// Every month on the 1st day, 9 AM and 30 minutes:
var monthlyCron = Cron.Monthly(1, 9, 30);
```

### Scheduling a task every n months:
```csharp
// Fires on 1st day every 3 months at 00:00
var everyThreeMonthsCron = Cron.MonthInterval(3);
```

### Scheduling yearly:
```csharp
// Every year on March 10th at midnight
var yearlyCron = Cron.Yearly(month:3, day:10);
``` 

---

## Exception Handling and Logging

- Olive's Cron methods include built-in validation for parameter values (e.g., minutes and hours are checked to ensure valid cron generation).
- Each method throws meaningful exceptions if provided with invalid parameter values outside logical intervals.

Example:
```csharp
var cronExpression = Cron.Hourly(70); // Throws ArgumentOutOfRangeException
``` 