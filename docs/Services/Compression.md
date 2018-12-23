# Olive.Compression

This library will get you *fluent methods* to compress and decompress your files and folders.

## Getting started

First you need to add the [Olive.Compression](https://www.nuget.org/packages/Olive.Compression/) NuGet package : `Install-Package Olive.Compression -Version 1.0.16` .
Now add `using Olive;` in top of your csharp file and start using `Olive.Compression` fluent methods!

### Compression

It's really easy to compress a folder. Just make a **DirectoryInfo** object then make a compressed file using `Compress()`extension method. Here is a full example:

```csharp
var folder = new DirectoryInfo("e:\\somewhere");
var file = new FileInfo("e:\\somewhereelse\\something.zip");
folder.Compress(CompressionFormat.Zip, file, overwrite: true);
```

You can compress your folder either in formats of **Zip** , **GZip** and **Tar**.

### Decompression

You can decompress your compressed file into a directory by making a **FileInfo** object then decompress it into a directory using `decompress()`extension method; Here is an example:

```csharp
var file = new FileInfo("e:\\somewhereelse\\something.zip");
var folder = new DirectoryInfo("e:\\somewhere");
file.Decompress(folder);
```

You can decompress archives with the formats of **Gzip**, **Zip**, **7Zip**, **Rar** and **Tar** with this fluent method.