# Development Process for Docker-Based Applications

## Development environment
Visual Studio 2017 comes with built in tools for Docker that let you develop, run, and test your app directly in the target Docker environment. You can press F5 to run it directly into a Docker host on your local machine, or press CTRL+F5 to edit and refresh your app without having to rebuild the container. 

By installing [Docker Community Edition (CE)](https://www.docker.com/community-edition) tools, you can use a single Docker CLI to build apps for both Windows and Linux.

### Workflow for developing Docker container-based applications 
This section describes the inner-loop development workflow. It is not taking into account the broader DevOps workflow or live deployment.

An application is composed of your own services plus additional libraries (dependencies). The following are the basic steps you usually take when building a Docker application, as illustrated in Figure 5-1.  

![Workflow](https://user-images.githubusercontent.com/1321544/50515111-f3f93800-0ab7-11e9-8c1c-4ad7de4a8049.jpg)

When you run the application in Visual Studio 2017, it builds the Docker image and runs the multi-container application directly in Docker; it even allows you to debug several containers at once. These features will boost your development speed. 

However, just because Visual Studio makes those steps automatic does not mean that you do not need to know what is going on underneath with Docker. If you were using an CLI development approach (for example, Visual Studio Code plus Docker CLI), you would need to know every step in more detail. Also in the case of problems you need to know what's going on. Therefore, in the guide we detail every step. 

#### Step 1. Start coding and create your app
Developing a Docker application is similar to the way you develop an application without Docker. 

#### Step 2. Create a Dockerfile
You normally need a Dockerfile for each microservice. It is placed in the root folder of its source code. It contains the commands that tell Docker how to set up and run your application or service in a container.

When you create a new M# Microservice project, this file is already included in the template. You can also enable Docker support on any new or existing project by right-clicking your project file in Visual Studio and selecting the option **Add-Docker Project Support**. 

To define the image for your app you start on top of a suitable base image from DockerHub such as `microsoft/aspnetcore:2.0` by adding the following line as the first line of your DockerFile:

- FROM microsoft/aspnetcore:2.0

The following shows a sample Dockerfile for an ASP.NET Core container. 

```csharp
FROM microsoft/aspnetcore:2.0 
ARG source 
WORKDIR /app 
EXPOSE 80 
COPY ${source:-obj/Docker/publish} .  
ENTRYPOINT ["dotnet", "Website.dll"] 
```
This example instructs Docker Host to listen on the TCP port 80 for incoming http requests. Also the **ENTRYPOINT** line above tells Docker to run a .NET Core application.

**Using multi-arch image repositories **
A single repo can contain platform variants, such as a Linux image and a Windows image. This feature allows vendors like Microsoft to create a single repo to cover multiple platforms (that is Linux and Windows). For example, the microsoft/aspnetcore repository provides support for Linux and Windows Nano Server. The actual OS will be determined when deploying, based on configuration of the Docker Host.

Alternatively you can specify a tag, targeting a platform explicitly:

| microsoft/aspnetcore:2.0.0-jessie | .NET Core 2.0 runtime-only on Linux   |
|:-:|:-:|
| microsoft/dotnet: 2.0.0-nanoserver | .NET Core 2.0 runtime-only on Windows Nano Server   |

#### Step 3. Build the image from Dockerfile
The Docker images are built automatically for you in Visual Studio when pressing F5. Underneath, it uses docker build command:

![Docker images](https://user-images.githubusercontent.com/1321544/50515340-3b33f880-0ab9-11e9-853e-98ad9b22ec53.jpg)

This will create a Docker image with the name `cesardl/netcore-webapi-microservicedocker:first`
- :first is a tag representing a specific version. 

You can find the existing images in your local repository by using the `docker images` command:

![docker images](https://user-images.githubusercontent.com/1321544/50515385-9fef5300-0ab9-11e9-97e5-71804c7451ce.jpg)

#### Step 4. multi-container Docker applications 
The [docker-compose.yml](https://docs.docker.com/compose/compose-file/) file lets you define a set of related services to be deployed as a composed application with deployment commands. You will not typically use this feature. 

#### Step 5. Build and run your Docker application
You can run the generated image by deploying it to your Docker using the docker run command. For example:
- docker run -t -d -p 80:5000 cesardl/netcore-webapi-microservice-docker:first

![Docker application](https://user-images.githubusercontent.com/1321544/50515417-e775df00-0ab9-11e9-8f43-9df2d4eb4f05.jpg)

It binds the internal port 5000 of the container to port 80 of the host machine. This means that the host is listening on port 80 and forwarding to port 5000 on the container. That is why you can open a browser in the host computer and send http requests to the app. 

### Running a Docker Container

#### Curl command
You can also test the application using curl from the terminal. On Windows, the default Docker Host IP is always 10.0.75.1 in addition to your machine's actual IP address. 

![Curl command](https://user-images.githubusercontent.com/1321544/50515461-21df7c00-0aba-11e9-9ad1-560253c4b0bb.jpg)

#### Development vs live run
The `docker run` command is adequate for testing containers in your development environment. But you should not use this approach if you are targeting Docker clusters and orchestrators like Kubernetes. That will be explained later. 

#### Debugging with Visual Studio 2017
When running and debugging the containers with Visual Studio, you can debug the application the same way as you would when running without Docker. But if you are developing using the editor/CLI approach, debugging containers is more difficult. For example you will have to debug by generating trace information in a file. 

### The Build Process

#### Building optimized ASP.NET Core Docker images 
In basic tutorials you will find Dockerfiles that demonstrate the simplicity of building a Docker image by copying your source into a container such as the following: 

```csharp
FROM microsoft/dotnet 
WORKDIR /app 
ENV ASPNETCORE_URLS http://+:80 EXPOSE 80 
COPY . . 
ENTRYPOINT ["dotnet", "run"] 
```

While that will work, but it's far from optimized. In the live environment when a container is started, it should be ready to run. You should not restore NuGet packages or compile the app at run time, which is that using `dotnet run` will do. 

The base image of `microsoft/dotnet` is a development environment which is far heavier than the running environment alternatives such as the [microsoft/aspnetcore](https://hub.docker.com/r/microsoft/aspnetcore/) image.

#### Building the application from a build server (CI)
Another benefit of Docker is that you can build your application from a preconfigured container so you do not need manually to create and configure a build machine  to build your application. You can create a Dockerfile for a build-specific container image, and run and test it at your development machine. Then you can use the image to create a container on our CI (Continuous Integration) server. 

For this scenario, Microsoft provides the [microsoft/aspnetcore-build](https://hub.docker.com/r/microsoft/aspnetcore-build/) base image, which you can use to compile and build your ASP.NET Core apps. It contains everything you need in order to compile an ASP.NET Core application, including .NET Core, the ASP.NET SDK, npm, Bower, Gulp, etc. The output is a run-time optimized image based on the [microsoft/aspnetcore](https://hub.docker.com/r/microsoft/aspnetcore/) base image. 

#### PowerShell commands in a Dockerfile
If you are only targeting Windows containers you can use powershell commands in your Docker file. For example:

```csharp
FROM microsoft/windowsservercore 
LABEL Description="IIS" Vendor="Microsoft" Version="10" 
RUN powershell -Command Add-WindowsFeature Web-Server 
CMD [ "ping", "localhost", "-t" ] 
```

In this case, we are using a Windows Server Core base image and then install IIS with a **PowerShell** command.

In a similar way, you could install any other Windows software. For example, to install ASP.NET 4.5: 

`RUN powershell add-windowsfeature web-asp-net45 `

