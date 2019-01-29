# Using AWS Services

Olive provides first class integration with several AWS services.
In addition, you can use any of the other AWS services using the [AWS SDK for .NET](https://aws.amazon.com/sdk-for-net/) directly in your projects.

Some of the built-in AWS plugins and implementations in Olive are:
- **Olive.Aws.EventBus**: Provides an implementation for the Olive `EventBus` abstractions based on Aws SQS.
- **Olive.Aws.Ses**: Provides an implementation for the Olive `EmailService` abstractions based on Aws SES.
- **Olive.BlobAws**: Provides an implementation for the Olive `Blob` abstractions based on Aws S3.
- **Olive.Security.Aws**: Provides an implementation for the .NET Core's `IDataProtector` abstraction based on Aws KMS.

In addition, there is a core enabler Olive component named **Olive.Aws** which takes care of the underlying connection and authentication for AWS Api Clients.

## Api Client Authentication
To access any AWS Api, your application needs to be authenticated with a valid AWS `IAM user` or `IAM role`.
To create one, you can log in to the AWS Console and create a user or role and associate the necessary permissions and policies to it.

>It is recommended that you use an **IAM Role** rather than a **IAM User** as that gives you more flexibility.

To claim that role in your application, you have 2 options:

- **EC2 Instance Role**: This is the recommended approach for production servers. In this approach, you do not need to use any username, password, token or any other *authentication data*. Instead, when creating the EC2 server instance, you will allocate the role directly to the server. Any calls made to the AWS Apis from your application, running on that server, will automatically be authenticated.

- **API Key pair**: In this approach you can generate an access key pair for the role on the AWS Console and add it to the application config files such as `appSettings.Development.json` using:

```json
{
  ...
   "Aws": {
     "Credentials": {
         "AccessKey": "...", 
         "Secret": "..."
     }
   }
}
