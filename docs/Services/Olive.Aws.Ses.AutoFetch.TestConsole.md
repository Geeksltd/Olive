# Olive.Aws.Ses.AutoFetch.TestConsole

## Overview
The `Program` class in this project is responsible for continuously fetching emails from an AWS SES mailbox. It sets up the necessary configuration, initializes the required services, and keeps polling for new emails.

## Configuration
Ensure the following configurations are set in your `appsettings.json`:
- **ConnectionStrings** - Required for initializing data access. 

## Class: `Program`

### `Main`
- **Summary**: Initializes the application and starts the email fetching process. 
- **Notes**:
  - Calls `Init()` to configure the application.
  - Starts watching the mailbox using `Mailbox.Watch("my-mailbox")`.
  - Continuously fetches new emails every second.

### `Init`
- **Summary**: Initializes the dependency injection container and database configuration. 
- **Steps Performed**:
  1. Loads `appsettings.json`.
  2. Configures logging to output to the console.
  3. Registers database-related services.
  4. Initializes the global `Context` for dependency resolution.
  5. Configures the database connection and access settings.

## Email Fetching Process

### `Mailbox.Watch` 
- **Summary**: Starts monitoring the specified AWS SES mailbox for new messages.
- **Usage**:
```csharp
await Mailbox.Watch("my-mailbox");
```
- **Notes**:
  - Automatically detects new emails arriving in the mailbox.

### `Mailbox.FetchAll`
- **Summary**: Fetches all emails from the monitored mailbox.
- **Usage**:
```csharp
await Mailbox.FetchAll();
```
- **Notes**:
  - Runs inside an infinite loop, fetching new messages every second. 

## Conclusion
The `Program` class sets up a fully automated AWS SES email fetching system. It continuously monitors and retrieves emails, leveraging dependency injection for service management. Ensure proper database configuration and AWS SES mailbox setup before running the application.