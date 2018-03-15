# Olive Microservice solution: Environment setup

A solution using Olive microservices will consist of a number of components that are needed either for run-time or development time.
This guide will explain what they are and how you can set them up to get started with your first Microservice based solution!

### AccessHub
If you want to benefit from the Composite UI architecture, then you need a UI container to orchestrate everything together.
AccessHub is an implementation of such UI container, or hub.

For each new solution (consisting of multiple microservices) you can simply:
1. Clone AccessHub from [here](https://gitlab.com/Geeks.Microservices/AccessHub)
2. Rename it (optional) after your solution
3. Configure its initial data based on your library of microservices
   - Add a Microservice record per microservice in your solution.
   - Add a Feature record per main navigation item in the solution.


### People Service
Almost every application will need a database to store the system users and their permissions. Often that database will have a custom structure, depending on the requirements of the application. [Learn more here](https://geeksltd.github.io/Olive/#/Microservices/Security?id=authorisation-via-people-service)

[Download a template from here.]()

### BigPicture
This is ...
Download the template from ...

### Build Server
Install Jenkins....
Private nuget repository....
