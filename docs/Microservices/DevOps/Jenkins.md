# Continious Integration using Jenkins

`Continious Delivery` and `DevOps` have many well known operational benefits for the process of software development and management. But they are even more vital in a `Microservice-oriented` architecture. Why? Because with Microservices, you will typically have `tens of deployments per day`, after every small bug fix, or new feature. That makes it impossible to go about it manually.

`Continious deployment` enables us to rapidly deploy changes without the risk of `human errors`. Once designed and tested, you can trust the deployment process way more than any developer, even the most experienced ones.

Also, `scripting the deployment process` is a form of `documenting` it, that enables anybody to learn and do the process easily.

## M# Apps: Basic Build Steps

> This guide is applicable to MC# projects. If you M# project is not converted to MC#, you should first do that.

The build process of MC# applications is [described here](http://learn.msharp.co.uk/#/Structure/README). Long story short, there is a build sequence, provided below, which your Jenkins file needs to follow to generate the application property: 

1. Build `#Model`
2. Build `Domain`
3. Build `#UI`
4. Build `Website`

Of course your application can have any other custom build steps. 

## Jenkins
There are many tools out there for deployment automation. We have chosen `Jenkins` because:

- It's very popular, with a strong community support.
- It's open source and free.
- It has a rich library of plugins for different build and deployment tasks.

### Jenkins Job
The first step to set up CI/CD in Jenkins is to create a `job` for the project. There are different types of jobs in Jenkins. We choose `Pipeline` jobs, because of their flexibility:

- They allow us to `script the build process`
- Pipelines provide `stages`, which split different build steps for better organization.
- Jenkins provides a very good presentation for different stages and their time elapsed.

### Jenkinsfile
A pipeline jobs is described using a `Jenkinsfile` which is based on the [Groovy](http://groovy-lang.org/) language.

Here you can see a [jenkinsfile template for M# projects]((Example-Jenkinsfile.md) along with a full description.

#### Location of the jenkinsfile
When creating a pipeline job, you should provide the jenkinsfile for it. Rather than uploading it to Jenkins directly, you can specify a GIT repository and a path within it, from which the build script (jenkinsfile) can be pulled.

Of course you can store the jenkinsfile for your project in the same GIT repo of the project. But for security reasons, we recommend creating a `separate repository` which is available only to users with the highest previlage.

In a microservice-oriented project, you can have a single repo to store all jenkins files for all of your microservices. There, you can have a `folder per microservice`, with a jenkinsfile inside it.


## Build Server

The `Build.bat` file requires some tools to be available on the server such as yarn, bower, webpack, TypeScript, ...

To automate that process, we have created a file named [PrepairServer.bat](Example-PrepairServer.bat.md). You should run it with every Jenkins build. But to avoid unnecessary reinstalling, it first checks to see if each tool is available on the server already, and if not it installs it.

> Note: This step can be removed if you create a build cluster using containers. That way, the build server's images will already have all the tools installed.
