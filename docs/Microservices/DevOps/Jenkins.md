# Continious Integration
`Continious Delivery` and `DevOps` have many well known operational benefits for the process of software development and management. But they are even more vital in a `Microservice-oriented` architecture. Why? Because with Microservices, you will typically have `tens of deployments per day`, after every small bug fix, or new feature. That makes it impossible to go about it manually.

`Continious deployment` enables us to rapidly deploy changes without the risk of `human errors`. Once designed and tested, you can trust the deployment process way more than any developer, even the most experienced ones.

Also, `scripting the deployment process` is a form of `documenting` it, that enables anybody to learn and do the process easily.

## Jenkins
There are many tools out there for deployment automation. We have chosen `Jenkins` because:

- It's very popular, with a strong community support.
- It's open source and free.
- It has a rich library of plugins for different build and deployment tasks.

### This guide
Our microservices commonly use the same technology stack (.net core, MS SQL) and are developed by [M#](http://learn.msharp.co.uk/#/Overview/README). This guide is intended to get you up and running with Jenkins for such projects quickly.

Though, you are not limited to use .net core or M#. You may choose whatever tech stack you need but the build process has to be adjusted for the new stack.

### Jenkins Job
The first step to set up CI/CD in Jenkins we need to first create a `job`. There are different types of jobs in Jenkins. We choose `Pipeline` jobs, because of their flexibility:

- They allow us to `script the build process`
- Pipelines provide `stages`, which split different build steps for better organization.
- Jenkins provides a very good presentation for different stages and their time elapsed.

### Jenkinsfiles 
Pipeline jobs are described using `Jenkinsfiles` which are based on the [Groovy](http://groovy-lang.org/) language. Here is an [example of a jenkinsfile](Example-Jenkinsfile.md).

#### Location of the jenkinsfile
Jenkins' pipeline jobs allow you to to specify a repository and a path in it, from which the build scripts can be pulled from. 
You can store the jenkinsfile for your project in the same GIT repo of the project, or in a separate repo. We recommend creating a `separate repository` where we store all microservice jenkins files. There you can have a `folder per microservice`, with a jenkinsfile inside it. 


## Build Steps

### M# build script (build.bat)
> This guide is applicable to MC# projects. If you M# project is not converted to MC#, you should first do that.

The build process of MC# applications is [described here](http://learn.msharp.co.uk/#/Structure/README). Long story short, there is a build sequence, provided below, which your Jenkins file needs to follow to generate the application property: 

1. Build `#Model`
2. Build `Domain`
3. Build `#UI`
4. Build `Website`

You can find the contents of the [Build.bat file here](Example-build.bat.md). 

## Build Server

As mentioned earlier, the Build.bat file described above requires some tools to be available on the server. To automate that process, everything we need to have has been added as scripts to the PrepairServer.bat file. We run this file everytime we run a Jenkins build process. The script doesn't reinstall anything. It first checks the server to see if the tool is available, if not it installs it.
This process can be removed once we create a build cluster using containers as all the build images will already have all the tools installed in them.

The tools we need to have installed to execute a build process are:
* yarn
* bower
* webpack
* TypeScript

Below is an example of a PreparirSserver.bat file which first installs `[Chocolatey](https://chocolatey.org/)` which is a package manager for Windows. It also adds the Chocolatey executable path to the PATH variable so that the "choco" command is available in the build process.
It installs `yarn and bower` using Chocolatey. It then installs `webpack` using yarn and TypeScript using npm.


```bat

@echo off

ECHO ::::::::: Ensuring Chocolatey is installed ::::::::::::::::::::::
WHERE choco > nul
if ERRORLEVEL 1 (
	ECHO ::::::::: Installing Chocolatey  ::::::::::::::::::::::
	call powershell -NoProfile -InputFormat None -ExecutionPolicy Bypass -Command "iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))" 
	call SET "PATH=%PATH%;%ALLUSERSPROFILE%\chocolatey\bin;"
	call choco feature enable -n allowGlobalConfirmation
	ECHO ::::::::: Installed Chocolatey  ::::::::::::::::::::::
) 


ECHO ::::::::: Ensuring Yarn installed (globally) ::::::::::::::::::::::
WHERE yarn > nul
if ERRORLEVEL 1 (
call choco install yarn
)

WHERE yarn > nul
if ERRORLEVEL 1 (goto error)


ECHO ::::::::: Ensuring Typescript compiler installed (globally) ::::::::::::::::::::::
WHERE tsc > nul
if ERRORLEVEL 1 (
call npm install -g typescript
)

WHERE tsc > nul
if ERRORLEVEL 1 (goto error)


ECHO ::::::::: Ensuring WebPack is installed (globally) ::::::::::::::::::::::
call yarn global add webpack
if ERRORLEVEL 1 (goto error)

ECHO ::::::::: Ensuring Bower (globally) ::::::::::::::::::::::
WHERE bower > nul
if ERRORLEVEL 1 (
call choco install bower
)

WHERE bower > nul
if ERRORLEVEL 1 (goto error)

exit /b 0

:error
echo ##################################
echo Error occured!!!
echo Please run Initialize.bat again after fixing it.
echo ##################################
set /p cont= Press Enter to exit.
PAUSE
exit /b -1

```
