
# Misc Extension Methods
>These are a bunch of useful extension methods which you can use in your applications.

## Shorten({Guid})
A normal `GUID` uses the characters '0' to '9', 'a' to 'f'. This is a range of 16 different characters. 
It also has generally 4 dashes that contain no data.
This method uses the characters '0' to '9', 'a' to 'z', 'A' to 'Z' and also '-' and '_'. This is a range of 64 characters.
Therefore a `shortGuid` is 22 characters long.
#### When to use it?
When you want to use a shorter `GUID` value in your applications.
#### Example:
```
Guid GID1 =new Guid("d7fedc56-959f-4d5b-8855-6138b534bce4");
GID1.Shorten(); // returns "Vtz-15-VW02IVWE4tTS85A"

Guid GID2 =new Guid("aceefe63-42f3-4135-a63a-96d1636f3b8d");
GID2.Shorten(); // returns "Y_7urPNCNUGmOpbRY287jQ"

Guid GID3 = Guid.Empty;
GID3.Shorten(); // returns "AAAAAAAAAAAAAAAAAAAAAA"
```

## ReadAllBytes({stream})
A `stream` is an object used to transfer data. There is a generic stream class `System.IO.Stream`, from which all other stream classes in .NET are derived. 
The `Stream` class deals with `bytes`.
#### When to use it?
When you want to read data from a stream, you should convert it to `Byte[]` object.
This method sets the Position to zero of a stream, and then copy all bytes to a memory stream's buffer.
>Asynchronous method : byte[] **ReadAllBytesAsync**(Stream)
#### Example:
```
byte[] array = File.ReadAllBytes("C:\\a");
        Console.WriteLine("First byte: {0}", array[0]);
        Console.WriteLine("Last byte: {0}", array[array.Length - 1]);
        Console.WriteLine(array.Length);
```

## NullIfDefault<T>({value})
Returns a `nullable` value wrapper object if this value is the default for its type.
#### When to use it?
When you want to compare value of an object with another value and return a `nullable` value if this value is the default for its type.
>Note 
>- The object should have a default value.
>- The type `T` must be a non-nullable value type in order to use it as parameter `T` in the generic type or method 'OliveExtensions.NullIfDefault<T>(T, T)'
#### Example:

```
double intdefault=0;
intdefault.NullIfDefault(0).ToString();  // returns Null

intdefault=100;
intdefault.NullIfDefault(100).ToString();  // returns Null

intdefault=10;
intdefault.NullIfDefault(100).ToString();  // returns 10

intdefault=100;
intdefault.NullIfDefault(10).ToString();  // returns 100

intdefault=10;
intdefault.NullIfDefault("abc").ToString();  // throws an exception : Error - string is not a non-nullable value type
```

## GetPath({string})
#### When to use it?
When you want to get the full path of a `file` or `directory` from a specified relative path.
#### Example:
In the client service application, you need the following code to enable you to create authenticated `HttpClient` instances which you can then use to invoke any actual `Web API`:
```
static FileInfo CachedTokenFile => AppDomain.CurrentDomain.**GetPath**("App_Data\\Temp\\token.txt").AsFile(); // returns token.txt as a file
```

## GetBaseDirectory()
#### When to use it?
When you want to get the fullpath of base directory in the server.
#### Example:
```
AppDomain.CurrentDomain.GetBaseDirectory(); // returns the path of the base directory of server
```

## LoadAssembly({assembly})
You do not have to put an `assembly` that an application must use at runtime in the bin folder of the application. You can put the `assembly` in any folder on the system, and then you can refer to the assembly at runtime.
#### When to use it?
When you want to get an Assembly object by knowing its path.
#### Example:
```
public Assembly Assembly { get; set; }

public Assembly GetAssembly() => Assembly ?? (Assembly = AppDomain.CurrentDomain.LoadAssembly('C:\Samples\AssemblyLoading\MultipleAssemblyLoading\f2\TestAssembly.dll'));
```
