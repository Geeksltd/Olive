# Olive Microservice solution: Environment setup

A solution using Olive microservices will consist of a number of components that are needed either for run-time or development time.
This guide will explain what they are and how you can set them up to get started with your first Microservice based solution!

### Microservice Explorer
This utility will help with creation and management of microservices using Olive.
[Read more here](https://github.com/Geeksltd/Olive.Microservice.Explorer/blob/master/README.md)

### AccessHub
If you want to benefit from the Composite UI architecture, then you need a UI container to orchestrate everything together.
[AccessHub](https://github.com/Geeksltd/Olive/blob/master/docs/Microservices/Overview.md#distributed-ui-via-access-hub) is an implementation of such UI container.

For each new solution (consisting of multiple microservices) you can simply:
1. Clone AccessHub from [here](https://gitlab.com/Geeks.Microservices/AccessHub)
2. Rename it after your solution (optional)
4. Configure your solution and navigation structure:
   - Add a Microservice record per microservice in `Services.xml` in your website project.
   - Add a Feature record per main navigation item in `Features.xml` in your website project.

> For integrations to work, you will also need to create a private nuget server, and [add it to your Visual Studio](http://nuget.geeksms.uat.co/nuget).

### People Service
Almost every application will need a database to store the system users and their permissions. Often that database will have a custom structure, depending on the requirements of the application. [Learn more here](https://geeksltd.github.io/Olive/#/Microservices/Security?id=authorisation-via-people-service)
<br/>You can clone People Service form [here](https://bitbucket.org/geeks-ltd/geeksms.people/src/master/)

### BigPicture
This project provides a bird's eye view to the solution. It contains information about:

- Individual services in the solution (Services.json)
- NugetServer (for hosting generated Api proxies)
- AWS Cluster setup info
- Kubernetes setup and config files
- Template files for microservice devops (jenkinsfile, dockerfile, ...)
- Utilities for automating AWS infrastructure setup (secrets, role, database, ...)

### Jenkins
This project contains the Jenkin files for all of your microservices.

### Build Server
You will need a BuildServer to install Jenkins, host private nuget repository, etc.
