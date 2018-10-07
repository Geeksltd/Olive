# Private Nuget repository

Different microservices often need to reference Api proxies from other microservices. The easiest approach is to use a private nuget server, so the publisher can simply generate its Api proxies into that central repo, and the consumer also download it from the same. It's also Build-Server friendly.

## Installing a Private Nuget server

There are various tools for creating a private nuget server such as [NuGet Server](http://nugetserver.net/) and [JFrog's Artifactory](https://jfrog.com/artifactory/).
You need to set this up once for your solution on a server that is available to the developers and also your build server.

in order to create a Nuget Server and package fee with **Nuget server** you need to fallow below steps:
1.	Create a new Empty **Asp.Net Web Application**.
2.	Choose a proper name for your project.
3.	Install [Nuget.Server]( https://www.nuget.org/packages/NuGet.Server/) Package form Nuget Package Manager  or Package Manager Console
4.	In the `web.config` set a unique value for **apiKey**. (It grants push access to the NuGet server )
5.  Run the project and your Private Nuget package server is ready.   

It is recommended to install your **Nuget Server** on your build server, next to Jenkins (or any other CI server that you use).
Access to this should be restricted to the permitted developers on the project.

## PrivatePackages folder

It is recommended to add a folder named PrivatePackages to the solution root folder.
You can use the path to that folder when generating Api proxies using the **Olive.Proxy.dll** utility.

Inside that folder you can add a file named Push.bat with the content of:

```
nuget push %1 {MySecureApiKey} -Source http://{MyNugetServerUrl}/nuget
```

Now every time that you generate a new Api proxy, you can just run this BATCH file and specify the package file name as its parameter.

```
C:\...\MySolution\PrivatePackages> push {NewPackageName}.nupkg
```

After pushing Nuget Packages to Nuget server it is time to add it as NuGet source and use it.
