# Migrating to CI/CD
This document covers the steps needed to migrate an existing project to the new CI/CD pipeline. Each step has been described in more details in their own documents which have been referenced here. Depening on what framework the project has been implemented by different steps/configurations should be done which have been highlighted. All the referenced documents explain everything for a microservice based application, which are applied to a monolith application assume that the application only has one big service.
It is assumed that you already have set up a VM as the build server.


## Build Server (Jenkins)
The build server requires some tools to be installed and configured to be able to carry out the build pipeline. You can find the instructions for setting up the build server in details [here](https://github.com/Geeksltd/Olive/edit/master/docs/DevOps/PreparingJenkinsServer.md)

## Cluster
Currently the CI/CD pipeline produces [docker images](https://github.com/Geeksltd/Olive/blob/master/docs/DevOps/Docker.md). Docker images should be run using a [container orchestrator](https://kubernetes.io/docs/concepts/overview/what-is-kubernetes/) in a cluster. [This document](https://github.com/Geeksltd/Olive/blob/master/docs/DevOps/Cluster-setup.md) describes how to create and set up a cluster from scratch.

## Updating the Application Resources
Once the build server and cluster are set up and ready, we need to prepare the project specific resources (i.e. Jenkinsfile and Kubernetes files) and include them in the source code.

## Git Structure
Becuase the Jenkins server is set up to work with a branch, everthing pushed to that branch will be built and deployed. As a best practice, it is better to have different branches for different environments. Below is the git structure recommended for having 3 environments with 2 of them having their own CI/CD proecess and one just CI.

1. Development (CI - not supported yet)
2. UAT (CI/CD)
3. Live (CI/CD)

Pangoline is not currently supported in our build pipeline but once fully introduced we can set up CI for the development branch. The structure is based on [GitFlow](https://datasift.github.io/gitflow/IntroducingGitFlow.html).
Unlike our defult git structure, which has only one master branch, this structure ensures not everything is included in each deployment unless explicitly merged to the relevant branch.
