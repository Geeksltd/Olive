# Olive.Aws

## Overview
Olive provides first-class integration with several AWS services, enabling seamless authentication and interaction with AWS APIs. You can also directly use any AWS service via the [AWS SDK for .NET](https://aws.amazon.com/sdk-for-net/).

This document covers the built-in AWS plugins in Olive, authentication mechanisms, role management, secret handling, and best practices.

---

## Built-in AWS Plugins in Olive
Olive includes several AWS-based implementations for common functionalities:

- **Olive.Aws.EventBus**: Implements `EventBus` using AWS SQS.
- **Olive.Aws.Ses**: Implements `EmailService` using AWS SES.
- **Olive.BlobAws**: Implements `Blob` storage using AWS S3.
- **Olive.Security.Aws**: Implements `.NET Core's IDataProtector` using AWS KMS.
- **Olive.Aws**: A core enabler component that manages authentication and connection setup for AWS API clients.

---

## AWS API Client Authentication
To access AWS APIs, your application must authenticate using an AWS IAM user or IAM role. It is recommended to use **IAM Roles** for flexibility and security.

### Authentication Methods
Olive supports three authentication methods:

### 1. API Key Pair (Development)
For local development, store AWS credentials in your `appSettings.Development.json`:

```json
{
   "Aws": {
     "Credentials": {
         "AccessKey": "...",
         "Secret": "..."
     }
   }
}
```

### 2. EC2 Instance Role (Production)
On production EC2 instances, IAM roles are attached directly, eliminating the need to manage credentials in config files.

### 3. EC2 Instance + Assume Role (For Multi-App Environments)
When multiple applications run on the same EC2 instance, define a role for the instance and separate roles for each application. Use `RuntimeIdentity` in Olive to assume application-specific roles dynamically:

```csharp
config.LoadAwsIdentity();
```

Switch between these authentication methods in `Startup.cs`:

```csharp
public Startup(IWebHostEnvironment env, IConfiguration config, ILoggerFactory factory)
  : base(env, config, factory)
{
    if (env.IsProduction()) config.LoadAwsIdentity();
    else config.LoadAwsDevIdentity();
}
```

---

## AssumeRole Class

### Purpose
The `AssumeRole` class provides methods to:
- Assume an AWS IAM role and set it as the default application identity.
- Generate temporary credentials for a given AWS role.
- Automatically renew assumed role credentials in the background.

### Usage

#### Assume a Role
```csharp
await AssumeRole.Assume("arn:aws:iam::123456789012:role/MyRole");
```
This will change the default application identity to the specified role in the specified AWS account.

#### Get Temporary Credentials
```csharp
var credentials = await AssumeRole.Temporary("arn:aws:iam::123456789012:role/MyRole");
```
This method returns temporary AWS credentials for the specified role without changing the application's identity.

#### Automatically Renew Credentials
```csharp
AssumeRole.KeepRenewing();
```
This starts a background process that renews the assumed role credentials every 5 minutes.

### Methods

#### `Task Assume(string roleArn)`
Sets the default application identity to the specified AWS IAM role and renews the credentials.

#### `Task<Credentials> Temporary(string roleArn)`
Returns temporary credentials for the given IAM role without setting it as the default identity.

#### `Task SignalRenew()`
Checks if credentials need renewal and renews them if they are older than 5 minutes.

#### `void KeepRenewing()`
Starts an infinite background process that renews credentials every 5 minutes.

#### `static async Task Renew()`
Internally used to renew credentials and reset AWS credential factories.

#### `static async Task<Credentials> LoginAs(string roleArn)`
Handles the actual `AssumeRole` AWS API request and logs the response status.

## AWS Extensions (`AWSExtensions` Class)

### Purpose
This class provides extension methods for `IConfiguration` to:
- Load AWS identity from environment variables or application settings.
- Load AWS credentials for development environments.
- Retrieve AWS Secrets from AWS Secrets Manager or AWS Systems Manager Parameter Store.

### Usage

#### Load AWS Identity from Environment Variables
```csharp
Configuration.LoadAwsIdentity();
```
This loads AWS credentials and secrets from environment variables.
This is intended for use in Production under Kubernetes pods as a workaround for adding AWS Roles to specific pods as opposed to the host server.
This is not needed in Lambda as you can allocate a role to lambda directly.

#### Load AWS Identity for Development
```csharp
Configuration.LoadAwsDevIdentity();
```
Loads credentials from `appsettings.json` under `Aws:Credentials:AccessKey` and `Aws:Credentials:Secret`.
Use this if you want to have AWS calls made under the current machine's Role,or the user specified in appSettings under (Aws { Credentials { AccessKey: ... , Secret: ... } }).

#### Load AWS Secrets
```csharp
Configuration.LoadAwsSecrets(SecretProviderType.SecretsManager);
```
Loads AWS secrets from AWS Secrets Manager.

### Methods

#### `void LoadAwsIdentity()`
Loads AWS credentials from environment variables and application settings.

#### `void LoadAwsDevIdentity(bool loadSecrets = false)`
Loads AWS credentials from `appsettings.json` and optionally loads AWS secrets.

#### `void LoadAwsDevIdentity(string accessKey, string secret, RegionEndpoint endpoint, bool loadSecrets)`
Explicitly loads AWS credentials using provided access key and secret.

#### `void LoadAwsSecrets(SecretProviderType provider = SecretProviderType.SecretsManager)`
Loads AWS secrets using the specified provider (Secrets Manager or Systems Manager Parameter Store).

## Runtime Identity (`RuntimeIdentity` Class)

### Purpose
Specifies the runtime IAM identity for an application and assumes the configured AWS role.

### Usage
```csharp
await RuntimeIdentity.Load(Configuration);
```
Loads the AWS IAM role from environment variables and assumes it.

### Methods

#### `static async Task Load(IConfiguration config)`
Loads and assumes the AWS IAM role specified in the environment variable `AWS_RUNTIME_ROLE_ARN`.

## Secrets Management (`Secrets` Class)

### Purpose
Handles fetching AWS Secrets from AWS Secrets Manager or Systems Manager Parameter Store.

### Usage
```csharp
var secrets = new Secrets(Configuration, SecretProviderType.SecretsManager);
secrets.Load();
```

### Methods

#### `protected override string DownloadSecrets()`
Retrieves secrets using the configured AWS secrets provider.

## Enums

### `SecretProviderType`
Defines the AWS secrets provider type:
- `SecretsManager`: AWS Secrets Manager
- `SystemsManagerParameter`: AWS Systems Manager Parameter Store

## Important Considerations

- **Security:** Do not hardcode AWS access keys and secrets. Always use IAM roles, environment variables, or AWS Secrets Manager.
- **Role Expiry:** AWS STS tokens expire. The application must handle token renewal using `KeepRenewing()`.
- **Error Handling:** Errors while assuming roles or fetching secrets should be logged and handled appropriately.

This document provides an overview of the AWS Assume Role and identity management utilities, ensuring secure and scalable authentication for AWS services.

