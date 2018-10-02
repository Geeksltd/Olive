
# Misc Extension Methods
>These are a bunch of useful extension methods which you can use in your applications.

## Shorten()
A normal `GUID` uses the characters '0' to '9', 'a' to 'f'. This is a range of 16 different characters. 
It also has generally 4 dashes that contain no data.
This method uses the characters '0' to '9', 'a' to 'z', 'A' to 'Z' and also '-' and '_'. This is a range of 64 characters.
Therefore a `shortGuid` is 22 characters long.
#### When to use it?
When you want to use a shorter `GUID` value in your applications.
#### Format:
ShortGuid **Shorten**(Guid)
#### Example:

|OBJECT                          |OUTPUT                       |
|-------------------------------|-----------------------------|
|d7fedc56-959f-4d5b-8855-6138b534bce4            |Vtz-15-VW02IVWE4tTS85A            |
|aceefe63-42f3-4135-a63a-96d1636f3b8d            |Y_7urPNCNUGmOpbRY287jQ            |
|Guid.Empty            |AAAAAAAAAAAAAAAAAAAAAA            |

## ReadAllBytes()
A `stream` is an object used to transfer data. There is a generic stream class `System.IO.Stream`, from which all other stream classes in .NET are derived. 
The `Stream` class deals with `bytes`.
#### When to use it?
When you want to read data from a stream, you should convert it to `Byte[]` object.
This method sets the Position to zero of a stream, and then copy all bytes to a memory stream's buffer.
#### Format:
byte[] **ReadAllBytes**()
>Asynchronous method : byte[] **ReadAllBytesAsync**(Stream)
#### Example:

|OBJECT                          |OUTPUT                       |
|-------------------------------|-----------------------------|
|MemoryStream            |Byte[]            |
|FileStream            |Byte[]            |

## NullIfDefault()
Returns a `nullable` value wrapper object if this value is the default for its type
#### When to use it?
When you want to compare value of an object with another value and return a `nullable` value if this value is the default for its type.
#### Format:
T? **NullIfDefault<T>**(T defaultValue = default(T))
>Note 
>- The object should have a default value.
>- The type `T` must be a non-nullable value type in order to use it as parameter `T` in the generic type or method 'OliveExtensions.NullIfDefault<T>(T, T)'

#### Example:

|OBJECT               |INPUT                          |OUTPUT                       |
|---------------------|-------------------------------|-----------------------------|
|0                    |0                              |NULL                         |
|100                  |100                            |NULL                         |
|10                   |100                            |10                           |
|100                  |10                             |100                          |
|10                   |"abc"                          |Error - string is not a non-nullable value type                       |
|"abc"                |10                             |Error - string is not a non-nullable value type                       |
|"abc"                |"abc"                          |Error - string is not a non-nullable value type                       |

## GetPath(string)
#### When to use it?
When you want to get the full path of a `file` or `directory` from a specified relative path.
#### Format:
**GetPath(string)**

#### Example:
In the client service application, you need the following code to enable you to create authenticated `HttpClient` instances which you can then use to invoke any actual `Web API`:
```
static FileInfo CachedTokenFile => AppDomain.CurrentDomain.**GetPath**("App_Data\\Temp\\...txt").AsFile();
```

## GetBaseDirectory()
#### When to use it?
When you want to get the fullpath of base directory in the server.
#### Format:
**GetBaseDirectory**()
#### Example:
```
AppDomain.CurrentDomain.GetBaseDirectory();
```

## LoadAssembly()
#### When to use it?
When you want to get an Assembly object by knowing its path.
#### Format:
**LoadAssembly**(string)
#### Example:
```
public Assembly Assembly { get; set; }
public Assembly GetAssembly() => Assembly ?? (Assembly = AppDomain.CurrentDomain.LoadAssembly(AssemblyName));
