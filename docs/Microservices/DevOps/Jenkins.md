# Continious Integration
`Continious Delivery` and `DevOps` have many well known operational benefits for the process of software development and management. But they are even more vital in a Microservice-oriented architecture. Why? Because with Microservices, you will typically have `10s of deployments per day`, after every small bug fix, or new feature. That makes it impossible to go about it manually.

`Continious deployment` enables us to rapidly deploy changes without the risk of `human errors`. Once designed and tested, you can trust the deployment process way more than any developer, even the most experienced ones.

Also, `scripting the deployment process` is a form of `documenting` it, that enables anybody to learn and do the process easily.


## Jenkins
There are many tools out there for deployment automation but we have chosen `Jenkins` because:

- It is one of the most popular tools, which means there is a good community support for it.
- It is open source, so we don't have to pay for anything.
- It has a rich library of plugins for different build and deployment tasks.

Our build process has a fairly complex structure and chosing Jenkins has been nothing but benefitial for us (hopefully it will remain that way). 

For our microservices commonly use the same technology stack (.net core, MS SQL) and developed by [M#](http://learn.msharp.co.uk/#/Overview/README); However, we are not limited to use .net core or M#. You may choose whatever tech stack you need but the build process has to be adjusted for the new stack.

### Jenkins Job
The first step to set up CI/CD in Jenkins we need to first create a `job`. There are different types of jobs in Jenkins but becaue of our complex build process we needed a good level of customizability, which `Pipeline` jobs offer. Apart form being able to script the build process, pipeline jobs offer stages, which can be used to split different build steps for better organization. Jenkins provides a very good presentation for different stages and their time elapsed.

### Jenkinsfiles 
There are two ways to manage Jenkins' build scripts for Pipeline jobs. The default way is to add the script to Jenkins but like any other code we want to be able to version control the build scripts to have visibility to changes overtime. Jenkins fortunately has a feature in Pipeline projects to specify a repository and a path in it, from which the build scripts can be pulled from. The file containing Jenkins build scripts are call `Jenkinsfiles`. Jenkinsfile are written in [Groovy](http://groovy-lang.org/), a bit scary eh! For our application we have create a repository where we store all microservice jenkins files in separate directories. Instead of different directories we perhaps could put all of them in one place and just call them {Service}_Jenkinsfile but having them in different directories hasn't caused any issues so far. 

Below is an example of a Jenkinsfile. Comments have been added to describe what each section is and does.

```groovy
pipeline 
{
    /// Build scripts normally have variables which have to be populated before the process starts. Jenkins by default provides
    ///  some useful ones such as BUILD_NUMBER or WORKSPACE (the physical path to where the project has been stored on the 
    /// build server by Jennkins) etc. Further variables can be defined as parameters or environment variables in jenkinsfile.
    /// The main difference between parameters and environment variables is that for parameters when you build a job Jenkins will 
    /// render a form where you can change the default values for each build.
    parameters 
    {
        string(name: "ParameterName" , defaultValue : 'Default value')                                    
    }
    environment 
    {   
        VARIABLE_NAME = "Static value"
        VARIABLE_NAME = "some static value + ${params.region}" // Value read from paramters       
    }
    agent any
    stages{    
            /// To be able to make sure a 100% clean build we run this stage to clear the workspace if there is anything in there.
            /// Later on we will try to create a serverless cluster for our build process and use containers as our agents which means
            /// this stage can be eliminated.
            stage('Delete the old version') 
            {
                steps
                {
                    script
                        {                       
                            deleteDir()
                        }
                }
            }
            
            /// We have a private repository for storing microservice specific api packages. Since it is a private repository
            /// it has to be introduced to nuget in the Jenkins executor.
            /// Same as the previous stage, once moved to a cluster our Jenkins slaves will not need to have this stage.
            stage('Add the private repository') 
            {
                steps
                {
                    script
                        {                       
                            sh ''' 
                                if [[ ! $(nuget sources) = *"${PRIVATE_REPOSITORY_ADDRESS_NAME}"* ]]; then 
                                    nuget sources Add -Name "${PRIVATE_REPOSITORY_ADDRESS_NAME}" -Source ${PRIVATE_REPOSITORY_ADDRESS} 
                                    echo "Installed the private repository"
                                else
                                    echo "The private repository already exists"
                                fi                                  
                               '''
                        }
                }
            }
                
            /// As the name suggests it closes the repository to local. For debugging purposes it helps to know that Jenkins pulls and 
            /// builds projects in %JENKINS_HOME%/workspaces/%PROJECT_NAME% where %JENKINS_HOME% can be found in the Jenkins Settings               /// menu of the Settings section of admin in Jenkins.
            stage('Clone sources') 
            {
                steps
                {
                    script
                        {                       
                            git credentialsId: GIT_CREDENTIALS_ID, url: GIT_REPOSITORY
                        }
                }               
            }
            
            /// This step updates the app release version in appsettings.json to the build number passed by Jenkins.            
            stage('Update settings') 
            {
                steps
                {
                    script
                        {           
                            echo "Update the application version"
                            sh ''' sed -i "s/%APP_VERSION%/${BUILD_NUMBER}/g" ./Website/appsettings.json '''                                                    
                        }                                       
                }
            }
            
            /// All references to javascript resources should point to Hub, for that we replace the baseUrl in the requirejs 
            /// file to the live hub url.
            stage('Update require js baseUrl') 
            {
                steps
                {
                    script
                        {                       
                            echo "Update require js baseUrl"
                            sh 'sed -i \'s#window\\[\\"BaseThemeUrl\\"\\] =.*;#window\\[\\"BaseThemeUrl\\"\\] = \\"\'${HUB_SERVICE_URL}\'\\";#gi;\' ./Website/wwwroot/scripts/references.js'
                        }
                }
            }               
            
            /// For production we don't need to run GCOP. To remove the overhead we just remove all GCOP references in all csproj files.
            stage('Delete the GCOP References')
            {
                steps 
                {
                    script 
                        {
                            bat 'nuget sources'
                            bat 'for /r %%i in (.\\*.csproj) do (type %%i | find /v "GCop" > %%i_temp && move /y %%i_temp %%i)'
                        }
                }
            }
            
            /// The build process - explained in the next stage - requires some tools to be installed on the server. This stage ensures             /// the tools are installed by running the "PrepareServer.bat" file provided in the DevOps\Jenkins 
            /// folder in the applciation.
            /// The prepareServer.bat is described in more details later in this article.
            stage('Prepare the server')
            {
                steps 
                {
                    script 
                        {
                            bat '''cd .\\DevOps\\Jenkins\\
                            call PrepairServer.bat'''
                        }
                }
            }
            
            /// This step runs the Build.bat file located in DevOps\Jenkins folder of the application which builds the application
            /// based using MSharp and dotnet cli.
            /// The Build.bat is described in more details later in this article.
            stage('Build the source code')
            {
                steps 
                {
                    script 
                        {
                            bat '''cd .\\DevOps\\Jenkins\\
                            call Build.bat'''     
                        }
                }
            }
            
            /// The build process publishes the website in "%WORKSPACE%/Website/publish".
            /// This step checks the published files to make sure that the website.dll has been generated.
            /// If not it stops the build process to avoid publishing an invalid version of the application.
            stage('Check build')
            {
                steps 
                {
                    script 
                    {
                        bat '''if exist "%WORKSPACE%/Website/publish/website.dll" (
                            echo Build Succeeded.
                            ) else (
                            echo Build Failed.
                            exit /b 1
                            )'''                      
                    }
                }
            }
            
            /// Our production environment runs on Linux nodes which means we need to generate linux docker images. For that we
            /// have to run the Linux version of docker. Because of this we have created a new Linux EC2 instance and added it
            /// as a slave node to Jenkins.
            /// To be able to share files between different nodes we have to stash the files/directories we need to share and
            /// unstash them on the destination node. To generate a working docker image for a service all we need is the 
            /// Dockerfile and the publish folder which is done in this stage.
             
            stage('Stashing artefacts')
            {
                steps
                {
                    dir(WORKSPACE)
                    {
                        stash includes: 'Dockerfile', name: 'Dockerfile'
                        stash includes: 'Website/publish/**/*', name: 'Website'
                    }
                }
            }
            
            /// As mentioned in the previous stage, Docker images are generated on Linux instances. This stage executes on
            /// the Linux instances, as specified by `node('LinuxDocker')`. Like the begining of the build process we want to make
            /// sure that the workspace is clean and this step does that for us.
            /// This stage can be removed onced we move to a build cluster.
            stage('Clear the workspace (on linux)')
            {
                steps 
                {
                    node('LinuxDocker')
                    {                        
                        script 
                            {   
                                cleanWs();
                            }
                    }
                }
            }
            
            /// This stage is executed on the Linux node. In the previous stage we cleared the workspace. Now we want to unstash
            /// the files we need to build the docker image.
            stage('Unstash artefacts (on linux)')
            {
                steps
                {
                    node('LinuxDocker')
                    { 
                        unstash 'Dockerfile'
                        unstash 'Website'
                    }
                }
            }
            
            /// This stage builds the docker image and tags it with IMAGE_NAME, provided as an environment variable 
            /// defined in the begining of this file.
            stage('Docker build (on linux)')
            {
                steps 
                {
                    node('LinuxDocker')
                    {                        
                        script 
                            {   
                                sh "docker build -t ${IMAGE_NAME} ."
                            }
                    }
                }
            }
            
            /// This stage uses the Jenkins AWS Credentials plugin to login using the aws credentials that has write
            /// access to `AWS ECR (Amazon Elastic Container Registry)`. Once logged in, it uses the docker plugin to push the docker image created in the 
            /// previous stage to the ECR repository.
            stage('Push to ECR (on linux)')
            {
                steps 
                {
                    node('LinuxDocker')
                    {        
                        script 
                        {
                            withAWS(credentials:AWS_CREDENTIALS_ID)
                            {
                                // login to ECR - for now it seems that that the ECR Jenkins plugin is not performing the login as expected. I hope it will in the future.
                                sh("eval \$(aws ecr get-login --no-include-email --region eu-west-1 | sed 's|https://||')")
                        
                                docker.withRegistry(ECR_URL, ECR_CRED) 
                                {
                                    docker.image(IMAGE).push(BUILD_VERSION)
                                }                       
                            }
                        }
                    }
                }
            }

            /// As described in the [Service Preparation](https://github.com/Geeksltd/Olive/blob/master/docs/Microservices/DevOps/Service-preparation.md) document, each service will have a 
            /// Deployment.yaml document which described the Kubernetes Deployment object metadata for that service.
            /// Every build will have to update the deployment's metadata in order to use the latest docker image created during 
            /// the build process. This stage uses the Deployment.yaml file as a template and creates a new version of it 
            /// with the updated docker image url and updates the pod metadata labels' version record with the build number of this
            /// build process, so that we can check what version of the application the running pods on production use.
            stage('Update the K8s deployment file')
            {
                steps 
                {        
                    script 
                    {
                        sh ''' sed "s#%DOCKER_IMAGE%#${ECR_IMAGE_URL}#g; s#%BUILD_VERSION%#${BUILD_VERSION}#g" < $K8S_DEPLOYMENT_TEMPLATE > $K8S_LATEST_DEPLOYMENT_FILE '''
                    }
                }
            }               
            
            /// After creating a new Kubernetes deployment file we are ready to publish the deployment and update the cluster with
            /// new version of the service. At the moment we update the production environment using the `recreate` strategy, which 
            /// terminates all the running pods for this service and creates new ones with the latest changes.
            /// Depending on our business needs, i.e. zero downtime, we can probably benefit from [other strategis](https://container-solutions.com/kubernetes-deployment-strategies/) available in 
            /// Kubernetes. 
            stage('Deploy to cluster')
            {
                steps 
                {
        
                    script 
                    {
                    
                        withCredentials([string(credentialsId: 'K8sCertificateAuthorityData', variable: 'K8S_CERTIFICATE_AUTHORITY_DATA'), string(credentialsId: 'K8sClientCertificateData', variable: 'K8s_CLIENT_AUTHORITY_DATA'), string(credentialsId: 'K8sClientKeyData', variable: 'K8s_CERTIFICATE_KEY_DATA')]) 
                        {
                        
                        kubernetesDeploy(
                            credentialsType: 'Text',
                            textCredentials: 
                            [
                                serverUrl: K8S_SSH_SERVER,
                                certificateAuthorityData: K8s_CERTIFICATE_AUTHORITY_DATA,
                                clientCertificateData: K8s_CLIENT_AUTHORITY_DATA,
                                clientKeyData: K8s_CERTIFICATE_KEY_DATA,
                            ],
                            configs: K8S_LATEST_CONFIG_FILE,
                            enableConfigSubstitution: true,
                        )
                        }
                    }
                }
            }               

        }
        
     post
    {
        always
        {
            /// This step deletes the docker image created during the build process. Since the image has already been pushed to the 
            /// remote repository, and specially from the security point of view, there is no point keeping a local version. 
            sh "docker rmi $IMAGE | true"
        }
    }
}
```

// TODO : We need to utilize some of the notification plugins Jenkins has to be able to give some feedback, whether a failure or a success, about the build process.
// TODO: We can potentially add a manual confirmation process right before the finla step of the deployment process to avoid accidential live deployments.

## Build Steps

### Msharp
If you have worked with M# or reviewed the link provided above you should know how it works. If you don't however, below is a short summary of how Msharp generates a project.

Each Msharp application has four projects. Two, `Model` and `UI`, used to define the metadata and structured used by MSharp to generate the application. The other two, `Domain` and `Website`, generated by Msharp using the metadata defined in Model and UI. After defining the application structure in Model and UI, you first need to build them using dotnet CLI, or in visual studio. There is a post-build-event defined in those two projects that run after they are built (duh!), which genrate the Domain and Website projects; However, there is a build sequence, provided below, you need to follow to generate the application property. 
* Model
* Domain
* UI
* Website

Because the UI project has to know what entities has been defined in the application it has a dependency to Domain. Once the Domain project is built, any changes to the entity structure - a new entity, new properties etc - made in Model will be visible to UI. The build process in Jenkins obviously has to follow the same order.
Below is the content of the Build.bat file. Comments have been added to explain each step. Bear in mind that the context in which this batch file is executed is the Jenkins workspace for the project, which looks similar to your local project directory. All paths are relative to the project workspace unless an absolute path is specified.

```bat


@echo off

/// The first step to build the application is to build the Model project. For that the below script navigates to the M#\Model directory and runs "dotnet build" to build the project. Once built, the domain project will be generated. At any point if there is an error the whole execution will stop and the Jenkins build process will terminate.
ECHO ::::::::: Building #Model :::::::::::::::::::::::::::::::::::::::::::::
cd ..\..\M#\Model
call dotnet build -v q
call :cleanUp

if ERRORLEVEL 1 (goto error)

/// This step navigates to the Domain project and builds the project by running "dotnet build".
ECHO.
ECHO ::::::::: Building Domain :::::::::::::::::::::::::::::::::::::::::::::
cd ..\..\Domain
call dotnet build -v q
call :cleanUp
if ERRORLEVEL 1 (goto error)

/// TODO: confirm the reasoning with Paymon.
cd ..\Website
ECHO.
ECHO ::::::::: Installing YARN :::::::::::::::::::::::::::::::::::
call yarn install
if ERRORLEVEL 1 (goto error)

/// This line makes sure that all the bower components (if any) referenced by the application are installed. Obviously bower components are not pushed to the repository and have to be pulled when publishing the application. 
ECHO.
ECHO ::::::::: Installing Bower components :::::::::::::::::::::::::::::::::
if exist bower.json (
call bower install
if ERRORLEVEL 1 (goto error)
)

/// This line makes sure that all the TypeScript files (if any) referenced by the application are installed. We do not push any generated files to the repository so we have this line here to generate the js files for our TypeScripts.
ECHO.
ECHO ::::::::: Building typescript files :::::::::::::::::::::::::::::::::
call tsc
if ERRORLEVEL 1 (goto error)

// Same as TypeScript files and Bower components the generated css files are not pushed to the repository. Sass files are compiled using the script below.
ECHO.
ECHO ::::::::: Building sass files :::::::::::::::::::::::::::::::::
call wwwroot\Styles\build\SassCompiler.exe compilerconfig.json
if ERRORLEVEL 1 (goto error)

ECHO.
ECHO ::::::::: Restoring Olive DLLs ::::::::::::::::::::::::::::::::::::
call dotnet restore
if ERRORLEVEL 1 (goto error)

/// After compiling the Domain project we are ready to build UI. The script below navigates to the UI folder in M# directory and builds the code by running "dotnet build" which will generated the website for us.
ECHO.
ECHO ::::::::: Building #UI ::::::::::::::::::::::::::::::::::::::::::::::::
ECHO.
cd ..\M#\UI
call dotnet build -v q
if ERRORLEVEL 1 (goto error)

/// At this stage, all the js, css and bower components have been generated and downloaded. The Website project has been generated and everything is ready for the final step which is publishing the website. The script below publishes the website into the "publish" directory. 
ECHO.
ECHO ::::::::: Publishing the website ::::::::::::::::::::::::::::::::::::::::::::::::
ECHO.
cd ..\..\Website
call dotnet publish -o publish

exit /b 0

/// TODO: do we need this?
:cleanUp
echo ##################################
echo Cleaning up!
RMDIR "obj" /S /Q
echo ##################################
exit /b -1

:error
echo ##################################
echo Error occured!!!
echo Please run Initialize.bat again after fixing it.
echo ##################################
set /p cont= Press Enter to exit.
exit /b -1

```


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
