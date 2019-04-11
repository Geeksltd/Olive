# Olive Encryption and Decryption

Some applications required to save data in the encrypted format for security reasons. Olive also facilitates the encryption and decryption of data.

## Implementation

There is an attribute `EncryptedProperty` which can be added to the Model Entity class.
     
### appsettings.config file

There should be a key for encryption and decryption in `appsettings.config` file under the Database node.

```json
"Database": {
    .....
    "DataEncryption": {
      "Key": "abcd1234"
    }
}
```

### Startup.cs file

Call this `Services.UsePropertyEncryption([DomainAssembly]);` in the `Configure` method and `services.AddEntityInterceptor();` in the `ConfigureServices` method. FYI, there is a generic overload which let you to have your encryption/decryption logic.

```c#
public override void ConfigureServices(IServiceCollection services)
{
    base.ConfigureServices(services);
    ...
    services.AddEntityInterceptor();
}

public override void Configure(IApplicationBuilder app)
{
    base.Configure(app);
    app.UsePropertyEncryption(typeof(User).Assembly);
}
```
### Example
We want to save **User's** first name in encrypted form.

```c#
public class User : EnitiyType
{
    public User()
    {
	    String("First name").Attributes("[EncryptedProperty]");
    }
}
```
    
### Generated Code

The generated code for the property.

```c#
[EncryptedProperty]
public string FirstName { get; set; }
```

### Saving Data
```c#
Database.Save<Domain.User>(new Domain.User { FirstName = "xyz" }); // this will save data in encrypted form.
```
### Fetching Data

```c#
Database.GetList<Domain.User>(); // data will be in decrypted form.
```
