# Docker and Microservices

Enterprises are increasingly realizing cost savings, solving deployment problems, and improving DevOps and production operations by using containers. Docker is the de facto standard in the container industry, supported by Windows and Linux.
In addition, the [microservices](https://martinfowler.com/articles/microservices.html) architecture is emerging as an important approach for mission-critical applications. In a microservice-based architecture, the application is built on a collection of services that can be developed, tested, deployed, and versioned independently.

This guide is an introduction to developing microservices-based applications and managing them using containers. It discusses architectural design and implementation approaches using .NET Core and Docker containers.

## What is Containerization?

Containerization is an approach to software development in which an application, its dependencies, and its configuration are packaged together as a container image. 
A containerized app can be tested as a unit and deployed as a container image instance to a host operating system. It enables you to deploy them across dev, staging and live environments with little or no modification.  

Containers also isolate applications from each other on a shared OS. Containerized applications run on top of a container host process that in turn runs on the OS (Linux or Windows). Containers have a significantly smaller footprint than virtual machine (VM) images. Each container runs a whole web app or a service.
Another benefit of containerization is scalability. You can scale out quickly by creating new containers for short-term tasks.

## What is Docker? 

**Docker** is an open-source project for automating the deployment of applications. Docker is also a company that promotes and evolves this technology. 
Docker image containers can run natively on Linux and Windows. On the development computer, you run a Docker host to run and debug your .net apps. On Windows you can create images for either Linux or Windows containers.  

A container image is a way to package an app or service and deploy it in a reliable and reproducible way. The benefit is that it makes the environment (dependencies) the same across different deployments. This means that you can debug it on your machine and then deploy it to another machine with the same environment guaranteed. 

You could say that Docker is not only a technology, but also a philosophy and a process. When using Docker, you will not hear developers say: “It works on my machine, why not in production?”, Instead, they can simply say, “It runs on Docker”. Since the packaged Docker application can run on any supported Docker environment, it will run the way it was intended to on all deployment targets (Dev, QA, staging, production, etc). 

### Docker vs Virtual Machines 
As containers require far fewer resources they are easy to deploy and they start fast. This allows you to run more services on the same hardware unit, thereby reducing costs.

![Docker vs Virtual Machines](https://user-images.githubusercontent.com/1321544/50485102-154f1b00-0a09-11e9-8f76-c8061bbeb48d.jpg)

### Docker glossary
**Dockerfile:** A text file that contains instructions for how to build a Docker image. It’s the description from which an Image will be created. 

**Container image:** A file on disk that is a package with all the dependencies and information needed to create a runnable container. You will normally have one Image per Microservice (i.e a small asp.net app). An image includes all the dependencies (such as frameworks) plus configuration to be used by a container runtime. Usually, an image derives from multiple base images that are layers stacked on top of each other to form the container filesystem. An image is immutable once it has been created. 

**Build:** The action of building a container image from a Dockerfile and the additional files in the folder where the image is built. You can build images with the Docker docker build command.

**Container:** A running instance of a Docker image. A container represents the execution of a single application  process. It consists of the contents of a Docker image and an execution environment. When scaling a service, you create multiple instances of a container from the same image.

**Docker host:** A process running on a Windows or Linux computer (physical or VM) that can create and run containers from images. Each container needs a host in order to run. It’s the runtime bridge between your local operating system and the container process. It provides an isolated world for the container to run in. When you install Docker on Windows, in Task Manager you can see `Docker for Windows` which is the Docker host on your machine.

**Repository (repo):** A collection of Docker images. Sometimes you need to group multiple images together for which you use repositories. For example some repos contain multiple variants of a specific image, such as a Windows image containing SDKs (heavier), a Windows image containing only runtimes (lighter) and a Linux image also containing only runtimes, Those variants can be marked with tags. 

**Tag:** A mark or label you can apply to images so that different images or versions of the same image can be identified. It can be used in Dockerfile and Compose files or repositories.

**Registry:** A service that provides access to repositories. The default registry for most public images is [Docker Hub](https://hub.docker.com/) (owned by Docker as an organization). A registry usually contains repositories from multiple teams. Companies often have private registries to store and manage images they’ve created. Azure and AWS also provide Container Registry services. 

**Compose:** A command-line tool and YAML file format with metadata for defining and running multi-container applications. For example if your application has two Dockerfiles (one for DB and one for web app) you can define a single .yml file to ‘compose’ the two containers as a single runnable unit. Then you can run that YML file with a single command (docker compose up) which creates and runs a container per image.

**Cluster:** A collection of Docker hosts exposed as if it were a single virtual Docker host, so that the application can scale to multiple instances of the services spread across multiple hosts within the cluster. Docker clusters can be created with Orchestrators such as Docker Swarm and Kubernetes. 

**Orchestrator:** A tool that simplifies management of clusters and Docker hosts. The de facto standard tool is Kubernetes. It enables you to manage the images, containers, and hosts through a command line interface (CLI) or a graphical UI. You can manage container networking, configurations, load balancing, service discovery, high availability, Docker host configuration, and more. An orchestrator is responsible for running, distributing, scaling, and healing workloads across a collection of nodes.

### Docker containers, images, and registries
When using Docker, you create an app or service and package it and its dependencies into a container image. An image is a static representation of the app or service and its configuration and dependencies.  

To run the app or service, the app’s image is instantiated to create a container, which will be running on the Docker host. Containers are initially tested in a development environment or PC.  

To go live, you should store images in a registry, which acts as a library of images and is needed when deploying to production orchestrators. Docker maintains a public registry via Docker Hub. But that’s public and not suitable for private software. Instead you will use private registry providers such as AWS or Azure. They are preferred when:
- Your images must not be shared publicly due to confidentiality. 
- You want to have minimum network latency between your images and your deployment environment. For example, if your production environment is Azure cloud, you probably want to store your images in Azure Container Registry so that network latency will be minimal. The same goes for AWS. 

### Kitematic (part of Docker Toolbox)
Kitematic is a user interface tool for managing Docker on your system. It allows you to easily search and pull images from Docker Hub to create and run your app containers. With Kitematic you can visually map ports, change environment variables, configure volumes, etc. Of course anything that you can do with it, you can do from the command line interface too. But to get started with and learn Docker, this can be a handy tool.

### Use containers for new projects 
Containers are commonly used in conjunction with a microservices architecture, although they can also be used to containerize web apps or services that follow any architectural pattern. You can use .NET Framework on Windows Containers, but the modularity and lightweight nature of .NET Core makes it perfect for containers and microservices architectures. When you create and deploy a container, its image is far smaller with .NET Core than with .NET Framework.

## Base container image layer: OS choice
When you create an Image for each app or service using a Dockerfile, you need to start with a base layer which is the operating system image. 

Given the diversity of operating systems supported by Docker and the differences between .NET Framework and .NET Core, you should target a specific OS and specific versions depending on the framework you are using.  

For Windows, you can use Windows Server Core or Windows Nano Server. They provide different characteristics (IIS in Windows Server Core versus a self-hosted web server like Kestrel in Nano Server) that might be needed by .NET Framework or .NET Core, respectively.  
For Linux, multiple distros are available and supported in official .NET Docker images (like Debian). 

![annotation 2018-12-27 191419](https://user-images.githubusercontent.com/1321544/50485718-a9ba7d00-0a0b-11e9-8597-439b41d45b58.jpg)


## Official .NET OS images 
