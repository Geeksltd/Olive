# Use AWS S3 as the temp folder to upload files

If you need to use S3 to be the temp storage when you are uploading a file you need to follow the following steps. But if you need to store the files in the S3 permanently, read [this](/Entities/Blob.md). Also, you may use both if you are hosting your service as a lambda function.

First things first, create a bucket which should have the following settings.
- It should be open for public to upload to or download from it.
- It should not let public get the list of the files.
- It should not keep the files for a long time. Let say 1 hour.
- There should be a user (AccessKey and Secret) which is allowed to list the files in the the bucket.

Next, add the following settings to the `appsettings.json`.
```json
...
  "Aws": {
    "Credentials": {
      "AccessKey": "ACCESS_KEY",
      "Secret": "SECRET"
    },
    "Region": "REGION",
    "TempBucket": "BUCKET_NAME",
  }
...
```
Then, in your `Startup.cs` call `AddS3BlobStorageProvider` in `ConfigureServices` before calling the base method.
```csharp
public override void ConfigureServices(IServiceCollection services)
        {
    ...
    services.AddS3BlobStorageProvider();

    base.ConfigureServices(services);
    ...
```

Finally, call `useS3FileUpload` in the `appPage.ts` like:
```ts
protected configureServices(services: ServiceContainer): void {
    this.useS3FileUpload(services, "URL_TO_THE_THEMP_BUCKET");
    super.configureServices(services);
}
```
Although, you can hardcode the url here but it's better to somehow get it from appsettigs. For instance you can have a hidden tag in your layout and read the value from there.