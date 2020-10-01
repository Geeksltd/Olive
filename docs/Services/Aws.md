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

To claim that role in your application, you have 3 options:

### API Key pair
This is useful during development time. In this approach you can generate an access key pair for the role on the AWS Console and add it to the application config files such as `appSettings.Development.json` using the following.

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
```

### EC2 Instance Role
This is the recommended approach for production servers. In this approach, you do not need to use any username, password, token or any other *authentication data*. Instead, when creating the EC2 server instance, you will allocate the role directly to the server. Any calls made to the AWS Apis from your application, running on that server, will automatically be authenticated.

The problem with this approach is that all applications running on a single EC2 server will have the same role. When hosting multiple applications on the same server, such as when using containerised microservices, you will need a modified version of this approached, explained below.

### EC2 Instance + Assume Role 
In this approach, you will define a specific role for the server, followed by other roles dedicated to each application (microservice) running on that server. You will then create a trust relationship between the server role and the application roles.

In Olive, a service called `RuntimeIdentity` will facilitate this for you. It is [explained here](./../DevOps/Security.md#aws-iam-role-for-pods). You can activate it by calling `config.LoadAwsIdentity()` as shown below.


You will often use a mix of the above two options and switch in between them depending on the runtime config in your `Startup.cs` file:
```csharp
public Startup(IWebHostEnvironment env, IConfiguration config, ILoggerFactory factory)
  : base(env, config, factory)
{
      if (env.IsProduction()) config.LoadAwsIdentity();
      else config.LoadAwsDevIdentity();
}
```

## Invoking custom AWS Apis
Once you enable the above authentication in your application, you can use any AWS SDK service without worrying about authentication. Every AWS Api functionality requires you to create some kind of `AwsXxxClient` object. The constructor of such client objects have multiple overloads. There is one default overload (with no argument) which is what you should use.

Beware: Most examples you will find online will use other overloads that need you to explicitly specify access key, secret or region. Ignore those parts, and always use the parameter-less constructor of the client object.
