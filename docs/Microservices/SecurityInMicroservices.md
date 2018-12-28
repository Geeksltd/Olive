# Security in Microservices

## Authentication and single-sign-on
In microservice scenarios, authentication is typically handled centrally. Learn how [this is implemented in Olive](https://geeksltd.github.io/Olive/#/Microservices/Security).

## Storing application secrets safely during development
To connect with protected cloud resources and other services, ASP.NET applications typically need to use secret tokens (keys, passwords, connection strings, etc). If you simply add them to the source code (e.g. appSettings.json file) this can open up security risks as sensitive information can become accessible to unauthorised team members.

### Storing secrets in environment variables 
One way to keep secrets out of source code to set string-based secrets as [environment variables](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets#environment-variables) on your development machines. For example, setting an environment variable `Logging:LogLevel:Default` to Debug would be equivalent to a configuration value from the following JSON file: 

```json
{ 
  "Logging": { 
    "LogLevel": { 
      "Default": "Debug" 
    } 
  } 
}
```

To access these values from environment variables, the application just needs to call `app.AddEnvironmentVariables()` in `StartUp.cs`.

### Storing secrets using Azure Key Vault 
Secrets added as environment variables are stored unencrypted on your machine, meaning anyone with access to your machine can get hold of them. That is not suitable for live and sensitive secrets. 

For live system, you should use a central and secure solution such as [Azure Key](https://azure.microsoft.com/en-us/services/key-vault/) Vault or AWS Key Management Service. These services provide sophisticated added security measures to protect your keys even in cases when your server is compromised!

