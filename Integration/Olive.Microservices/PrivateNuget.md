# Private Nuget repository

Different microservices often need to reference Api proxies from other microservices. The easiest approach is to use a private nuget server, so the publisher can simply generate its Api proxies into that central repo, and the consumer also download it from the same. It's also Build-Server friendly.

## Installing a Private Nuget server
There are various tools for creating a private nuget server. You can download an example here.
You need to set this up once for your solution on a server that is available to the developers and also your build server.

It is recommended to install it on your build server, next to Jenkins (or any other CI server that you use).
Access to this should be restricted to the permitted developers on the project.

In the **web.config** of that application, you need to set a unique value for **apiKey** which grants push access to the nuget server.

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
