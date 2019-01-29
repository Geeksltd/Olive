# Using AWS Services

Olive provides first class integration with several AWS services.
In addition, you can use any of the other AWS services using the [AWS SDK for .NET](https://aws.amazon.com/sdk-for-net/) directly in your projects.

Some of the built-in AWS plugins and implementations in Olive are:
- **Olive.Aws.EventBus**: Provides an implementation for the Olive `EventBus` abstractions based on Aws SQS.
- **Olive.Aws.Ses**: Provides an implementation for the Olive `EmailService` abstractions based on Aws SES.
- **Olive.BlobAws**: Provides an implementation for the Olive `Blob` abstractions based on Aws S3.
- **Olive.Security.Aws**: Provides an implementation for the .NET Core's `IDataProtector` abstraction based on Aws KMS.

In addition, there is a core enabler Olive component named **Olive.Aws** which takes care of the underlying connection and authentication for AWS Api Clients.
