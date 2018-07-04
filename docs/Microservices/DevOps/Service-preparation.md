# Preparing your microservice for deployment

This document explains the steps needed in order to prepare your service to be deployed to the production environment. Before going through this document please make sure you have covered the [Cluster Setup](https://github.com/Geeksltd/Olive/blob/master/docs/Microservices/DevOps/Cluster-setup.md), [Docker](https://github.com/Geeksltd/Olive/blob/master/docs/Microservices/DevOps/Docker.md), [Jenkins](https://github.com/Geeksltd/Olive/blob/master/docs/Microservices/DevOps/Jenkins.md) and [Security](https://github.com/Geeksltd/Olive/blob/master/docs/Microservices/DevOps/Security.md) documents to get a better undestanding of the production environment and the CI/CD processes.

In order to deploy a service we need to be able to build the code and create the docker image and the resources we need to be able to update the production cluster, so let's start off with setting up the CI/CD in Jenkins.

## CI/CD
For more information about how the CI/CD project works please refere to [this](https://github.com/Geeksltd/Olive/blob/master/docs/Microservices/DevOps/Jenkins.md) document. 
We need the following files to be able to setup Jenkins. 
* Jenkinsfile (used by Jenkins)
* Deployment.yaml (for Kubernetes)
* Sercret.yaml (for Kubernetes)
* Service.yaml (for Kubernetes)

To automate, and possibly avoid human error, creating those files a script has been provided in the BigPicture repository in /DevOps called ContinuousDelivery.bat. In the same folder there are two other folders called Jenkins and Kubernetes which contains the templates used by ContinuousDelivery.bat script to generate the DevOps files. When running the script you need to provide your service name. This can be achived by passing the name with -serviceName argument when calling the script. Bear in mind that the name you provide here will be used in the CI/CD process and all the resources such as Service, Deployment, Labels on Kubernetes. 
When you run the script it creates a directory called Generated in the same directory as the script file. You need to copy the Generated/YourServiceName folder to the root of the Jenkins repository and rename it to DevOps so that the Jenkins job we create later on in this document can have access to it.

## 1: Create a build script

...
Bat


## 2: Define your service in AWS

