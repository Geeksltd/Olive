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

Docker is an open-source project for automating the deployment of applications. Docker is also a company that promotes and evolves this technology. 
Docker image containers can run natively on Linux and Windows. On the development computer, you run a Docker host to run and debug your .net apps. On Windows you can create images for either Linux or Windows containers.  

A container image is a way to package an app or service and deploy it in a reliable and reproducible way. The benefit is that it makes the environment (dependencies) the same across different deployments. This means that you can debug it on your machine and then deploy it to another machine with the same environment guaranteed. 

You could say that Docker is not only a technology, but also a philosophy and a process. When using Docker, you will not hear developers say: “It works on my machine, why not in production?”, Instead, they can simply say, “It runs on Docker”. Since the packaged Docker application can run on any supported Docker environment, it will run the way it was intended to on all deployment targets (Dev, QA, staging, production, etc). 
