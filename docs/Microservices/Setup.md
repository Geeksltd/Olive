# Olive Microservice solution: Environment setup

A solution using Olive microservices will consist of a number of components that are needed either for run-time or development time.
This guide will explain what they are and how you can set them up to get started with your first Microservice based solution!

### AccessHub
If you want to benefit from the Composite UI architecture, then you need a UI container to orchestrate everything together.
[AccessHub](https://github.com/Geeksltd/Olive/blob/master/docs/Microservices/Overview.md#distributed-ui-via-access-hub) is an implementation of such UI container.

For each new solution (consisting of multiple microservices) you can simply:
1. Clone AccessHub from [here](https://gitlab.com/Geeks.Microservices/AccessHub)
2. Rename it (optional) after your solution
3. Add required package sources. 
<br/>if it is your first time using AccessHub, you may need to add some package sources:
   -	Add http://nuget.geeksms.uat.co/nuget  as a NuGet package source to access peopleService API Proxies.
   -	Add http://nuget.gcop.co/nuget as a NuGet Package source to access GCop. For more information see this [link](https://github.com/Geeksltd/GCop/tree/master).
You need to make sure that suitable package source is selected in your NuGet package manager when you want to install or update packages.
4. Configure its initial data based on your library of microservices
   - Add a Microservice record per microservice in Services.xml in your website project.
   - Add a Feature record per main navigation item in Features.xml in your website project.You can also add a recored per sub-navigation item inside your Feature recored.
   
Now you can run the AccessHub and log in via Simulate login. But before you log into AccessHub, you need to run People Service first.

### People Service
Almost every application will need a database to store the system users and their permissions. Often that database will have a custom structure, depending on the requirements of the application. [Learn more here](https://geeksltd.github.io/Olive/#/Microservices/Security?id=authorisation-via-people-service)
<br/>You can clone People Service form [here](https://gitlab.com/Geeks.Microservices/People)
### BigPicture
This is ...
Download the template from ...

### Build Server
Install Jenkins....
Private nuget repository....
