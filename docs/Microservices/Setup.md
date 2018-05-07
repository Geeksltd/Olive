# Olive Microservice solution: Environment setup

A solution using Olive microservices will consist of a number of components that are needed either for run-time or development time.
This guide will explain what they are and how you can set them up to get started with your first Microservice based solution!

### AccessHub
If you want to benefit from the Composite UI architecture, then you need a UI container to orchestrate everything together.
[AccessHub](https://github.com/Geeksltd/Olive/blob/master/docs/Microservices/Overview.md#distributed-ui-via-access-hub) is an implementation of such UI container.

For each new solution (consisting of multiple microservices) you can simply:
1. Clone AccessHub from [here](https://gitlab.com/Geeks.Microservices/AccessHub)
2. Rename it after your solution (optional)
3. If it's your first time using AccessHub, you need to:
   - [Add GCop NuGet source](https://github.com/Geeksltd/GCop).
   - [Add your solution's nuget source](http://nuget.geeksms.uat.co/nuget).
   
4. Configure your solution and navigation structure:
   - Add a Microservice record per microservice in `Services.xml` in your website project.
   - Add a Feature record per main navigation item in `Features.xml` in your website project.
   
5. Now you can run the AccessHub and log-in (via the *Simulate login* feature).

### People Service
Almost every application will need a database to store the system users and their permissions. Often that database will have a custom structure, depending on the requirements of the application. [Learn more here](https://geeksltd.github.io/Olive/#/Microservices/Security?id=authorisation-via-people-service)
<br/>You can clone People Service form [here](https://gitlab.com/Geeks.Microservices/People)

### BigPicture
This is ...
Download the template from ...

### Build Server
Install Jenkins....
Private nuget repository....
