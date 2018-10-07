# Blob (file storage) in Olive

Entities often have several simple-typed proeprties such as string, int, DateTime, etc. But sometimes you need to store one or more file-typed properties for an entity. For example you might have an `Employee` class with file-based properties for *Resume*, *Photo*, etc.

For each file, you need to know the file name, its extension, and of course the actual binary data. As the binary data could be quite large, rather than storing it in the database directly you may want to store the files on disk or use a cloud provider such as AWS S3 or Azure Blob Storage service.

To solve these problems, Olive provides a class named `Blob` in the *Olive.Entities* namespace. A property of type Blob, represents a file-based property in entity classes. 

Each instance of a Blob object encapsulates the original file name, the entity that owns this file, and the property name. It also provides many methods and functionality for easy creation and manipulation at run-time.

## Configuration
The default Olive project template provides the following settings in *appSettings.json*.

```json
...
"Blob": {
        "RootPath": "Blob",
        ...        
},
```

By default, the physical files will be stored and read from the disk, from the specified root path. For example, let's say you have an entity named `Employee` with a property named `Resume`. You have an Employee instance with the id of *123...9*. If a file is uploaded named *MyResume.pdf* then the physical path of the blob storage will be: 

>\[Website root\]\\Blob\\Employee.Resume\\123...9.pdf.

## IBlobStorageProvider
The actual storage requirement for the files may vary from project to project. By default, in Olive the physical files will be stored and read from the disk, from the path specified in *appSettings.json* which is explained later.

You can, however, change the storage implementation based on your requirements. You can create any class that implements the `IBlobStorageProvider` to achieve this. Then to register your implementation, you can use the built-in ASP.NET Core dependency injection service.

```c#
  ...
  public override void ConfigureServices(IServiceCollection services)
  {
       base.ConfigureServices(services);
       ...
       
       services.AddSingleton(typeof(IBlobStorageProvider), new MyStorageProvider());
  }
```

### Using AWS S3
Cloud storage is recommended for production applications, rather than a local file system. There are several implementations of `IBlobStorageProvider` on NuGet already. For example, you can install [Olive.BlobAws](https://www.nuget.org/packages/Olive.BlobAws/) via NuGet in your project, and then in your `ConfigreServices(...)` method simply call the extension method that sets your storage implementation to AWS S3:

```c#
  ...
  public override void ConfigureServices(IServiceCollection services)
  {
       base.ConfigureServices(services);
       ...
       
       services.AddS3BlobStorageProvider();
  }
```

### Configuration
For the above to work, you need to add the necessary S3 settings to *appSettings.json* file.
```json
...
"Blob": {
        "S3": {
           "Bucket": "{your bucket name}",
           "Region": "{your bucket region}"
        }
        ...        
},
```
