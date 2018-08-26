# Ci/CD for Legacy ASP.NET apps

The [Jenkins.md](Jenkins.md) describes the build process in details. This document includes the Jenkinsfile steps for building and automating a `ASP.Net MVC 5` and `Web Forms` projects.

> Configuring the Jenkinsfile as below doesn't require the Build.bat file in the repository.

## Variables

| Variable  | Value to set |
| ------------- | ------------- |
| `GIT_REPO_SSH` | The SSH address of the git repository. |
| `GIT_BRANCH` | The name of the branch that will be used to build and deploy the application. |
| `GIT_CREDENTIALS_ID` | The ID of the SSH credentials record in Jenkins. The details will be provided below. |
| `K8S_SSH_SERVER`  | The url of the cluster. Can be found in the kubernetes config file in ~/.kube/.config  |
| `ECR_URL` | The url of the AWS container registry. |

## Placeholders
Replace the following placeholders in the jenkinsfile with the correct values for your project:

| Placeholder  | Value to use |
| ------------- | ------------- |
| `#DOCKER_REPOSITORY_NAME#`  | The name of the container repository, on the container registry, where the docker images will be stored.  |
| `#CONNECTION_STRING_CREDENTIALS_ID#` | The ID of the credentials record created for the connectionstring. You can store connectionstrings as a text credentials record in Jenkins. |
| `#WEBSITE_DDL_NAME#` | The name of the website compiliation output. |
| `#CONTAINER_REPOSITORY_CREDENTIALS_ID#` | The ID of the container repository credentials. It should be a username/password credentials in Jenkins. |
| `#AWS_CREDENTIALS_ID#` | The ID of the AWS credentials. It should be a username/password credentials in Jenkins.|


## The Jenkinsfile

```groovy
pipeline 
{
    environment 
    {   
        GIT_REPO_SSH = "..."
        GIT_CREDENTIALS_ID = "..."
        GIT_BRANCH = "..."
        ECR_URL = "..."
        
        BUILD_VERSION = "v_${BUILD_NUMBER}"
        IMAGE = "#DOCKER_REPOSITORY_NAME#:${BUILD_VERSION}" 

        K8S_SSH_SERVER = "..."
        K8S_DEPLOYMENT_TEMPLATE = ".\\DevOps\\Kubernetes\\Deployment.yaml"
        K8S_LATEST_DEPLOYMENT_FILE = ".\\DevOps\\Kubernetes\\Deployment${BUILD_VERSION}.yaml"               
        K8S_LATEST_CONFIG_FILE = "DevOps/Kubernetes/Deployment${BUILD_VERSION}.yaml"
        PATH = "C:\\Nuget;C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\BuildTools\\MSBuild\\15.0\\Bin\\;$PATH"
        CONNECTION_STRING = credentials('#CONNECTION_STRING_CREDENTIALS_ID#');      
    }
    agent any
    stages
        {   
            stage('Delete prev build folder') { steps { script { deleteDir() } } }
                        
            stage('Git clone sources') { steps { script {
                 git branch: GIT_BRANCH, credentialsId: GIT_CREDENTIALS_ID, url: GIT_REPO_SSH
            }}}
            
            stage('Prepare the server') { steps { dir("\\DevOps\\Jenkins") { bat 'PrepairServer.bat' } } }
            
           // This stage populates dynamic placeholders (application version number, connection strings etc) in the source code. 
           stage('Update settings') { steps { script {
                dir("Website")       
                { sh """ sed -i "s/%APP_VERSION%/${BUILD_NUMBER}/g;s/%CONNECTION_STRING%/${CONNECTION_STRING}/g;" web.config """ }
           }}}
            
           stage('Remove GCOP') { steps { script {
                bat 'nuget sources'
                powershell 'DevOps/DeleteGCopReferences.ps1'
                bat 'for /r %%i in (.\\*.config) do (type %%i | find /v "GCop" > %%i_temp && move /y %%i_temp %%i)'
           }}} 
            
            stage('Restore nuget packages') { steps { bat 'nuget restore' } }
            
            stage('Build the source code')
            {
                steps 
                {
                    script { dir("\\MsharpBuild") { bat 'msbuild' } }
                    script { dir("\\MsharpBuild\\Msharp") { bat 'Msharp.DSL.exe /build /model %workspace%' } }
                    script { dir("\\MsharpBuild\\Msharp") { bat 'Msharp.exe /build /model %workspace%' } }
                    script { dir("\\MsharpBuild\\Msharp") { bat 'Msharp.exe /build /ui %workspace%' } }                        
                }
            }          
            stage('Prepare runtime resources (bower components, scripts, css)')
            {
                steps 
                {
                    dir("Website")
                    {
                        script { bat 'bower install' }
                        script 
                        {                    
                            bat 'npm install gulp'
                            bat 'gulp less-to-css'                          
                            bat 'gulp fonts'
                        }
                    }
                }           
            }
            
            // This stage is mandatory only for MVC projects.
            stage('Publish the website')
            {
                steps { script { dir("Website") { bat 'MSBuild' } } }
            }
            
            // This stage checks to see if the build process has successfully created the webiste dll. It checks the website binary folder path for [#WEBSITE.DDL.NAME#].dll
            stage('Check build')
            {
                steps 
                {
                    script 
                    {
                        dir("Website")
                        {
                            bat '''if exist "bin\\[#WEBSITE_DDL_NAME#].dll" (
                                echo Build Succeeded.
                                ) else (
                                echo Build Failed.
                                exit /b %errorlevel%
                                )'''                        
                        }
                    }
                }
            }
            // This stage builds and pushes the docker image to a docker repository. Below are different scripts for pushing the Docker image to Docker Hub and AWS Contianer Registry
            // Make sure you have a credentials created in Jenkins with "CONTAINER_REPOSITORY_CREDENTIALS_ID" ID. 
            stage('Build and publish the docker images') 
            {
                
                steps 
                {
                    script 
                        {
                        // Docker Hub
                            docker.withRegistry("","CONTAINER_REPOSITORY_CREDENTIALS_ID") 
                            {   
                                docker.build(IMAGE).push();                                                         }
                        // AWS Docker Registry
                         script 
                        {
                            withAWS(credentials:#AWS_CREDENTIALS_ID#)
                            {
                                // login to ECR - for now it seems that that the ECR Jenkins plugin is not performing the login as expected. I hope it will in the future.
                                sh("eval \$(aws ecr get-login --no-include-email --region eu-west-1 | sed 's|https://||')")
                        
                                docker.withRegistry(ECR_URL, "CONTAINER_REPOSITORY_CREDENTIALS_ID") 
                                {
                                   docker.build(IMAGE).push();
                                }                       
                            }
                        }
                        }    
                }   
        
            }
        // To be able to update the Kubernetes cluster we need to upload a deployment template. There is a deployment file with some placeholders in the source code repository which is populated by this stage with the build information such as build version and the new docker image reference.
            stage('Update the K8s deployment file')
            {
                steps 
                {        
                    script 
                    {
                        sh ''' sed "s#%DOCKER_IMAGE%#${IMAGE}#g; s#%BUILD_VERSION%#${BUILD_VERSION}#g" < $K8S_DEPLOYMENT_TEMPLATE > $K8S_LATEST_DEPLOYMENT_FILE '''
                    }
                }
            }
            stage('Deploy to cluster')
            {
                steps 
                {
        
                    script 
                    {
                    
                        withCredentials([string(credentialsId: '#K8S_CLUSTER_CERTIFICATE_AUTHORITY_DATA_CREDENTILAS_ID#', variable: 'K8S_CERTIFICATE_AUTHORITY_DATA'), string(credentialsId: 'K8S_CLUSTER_CLIENT_CERTIFICATE_DATA_CREDENTIALS_ID', variable: 'K8s_CLIENT_AUTHORITY_DATA'), string(credentialsId: 'K8S_CLUSTER_CLIENT_KEY_DATA_CREDENTIALS_ID', variable: 'K8s_CERTIFICATE_KEY_DATA')]) 
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
            // make sure that the Docker image is removed
            sh "docker rmi $IMAGE | true"
        }
    }
}


```

### Tips

- `PrepairServer.bat` - This file installs all the tolls required to biuld the application. Dockerising the Jenkins server will eliminate this stage from the build process and shorten it.


  
#### Kubernetes Cluster Placeholders.
To be able to connect to Kubernetes you need to extract the certificate information of the use you want to connect to the cluster with. The credentials details can be found in ~/.kube/config. There are three parts you need to extract from the config file and import as *"text credentials"* in Jenkins.
- K8S_CLUSTER_CERTIFICATE_AUTHORITY_DATA_ID
  - The value of "certificate-authority-data" key in the config file.
- K8S_CLUSTER_CLIENT_CERTIFICATE_DATA_CREDENTIALS_ID
  - The value of "client-certificate-data" key in the config file.
- K8S_CLUSTER_CLIENT_KEY_DATA_CREDENTIALS_ID
  - The value of the "client-key-data" key in the config file.


### Access Git via SSH
A safe way to pull the source code from the git repository is to use SSH. For that we need to generate a keypair on the build server and import the private key in Jenkins as a "SSH username and private key" record. You can easily generate a key pair by running the "ssh-keygen -t rsa -b 4096". You then need to provide a filepath you want the generated keys to be stored as and a password (make a note of the password as you will need it shortly) . Once generated, go to Jenkins > Credentilas and add a new "SSH username and private key" record. Put "git" as the username and select "Enter directly" for the Private Key option. This allows you to put the content of the private key file generated earlier. The next field is the password you specified during the key generation process, and finally you need to set an ID and description for the key. 
The next step is to import the public key to the git repository. Depending on what repository you use there will be different instructions. For example you can follow the below instruction if you use Bitbucket.

- Go to Bitbucket and select "Bitbucket settings" from your avatar in the lower left.
- The Account settings page opens.
- Click SSH keys.
- If you've already added keys, you'll see them on this page.
- Click Add key.
- Enter a Label for your new key, for example, Default public key.
- Paste the copied public key into the SSH Key field.


### Docker files
Docker requires a dockerfile with the instructions needed to build a docker image. You can use the template below as the starting point and customize it if needed. You need to create a file called "Dockerfile" and save the content below to it. **The Dockerfile should be save in the root directory of the project.**

```
FROM microsoft/aspnet:4.7.2-windowsservercore-ltsc2016

COPY Website /inetpub/wwwroot
```

The first line of the script above sets the base image of the your docker image. Depending on the framework you have used in your application the base image may vary. Having a wrong base image might result in not being able to run your application in a container. The second line copies the Website folder into /inetpub/wwwroot folder in the container. The default IIS in microsoft/aspnet docker image points to /inetpub/wwwroot. 

**Important**
Just like a git repository, you should avoid adding unnecessary files to your docker image. Copying only the files you require to run the application reduces the size of the docker image and make it more efficient and faster to pull and run it. To avoid copying everything you can use .dockerignore files. It has the same pattern as .gitignore. One of the most import folders you need to ignore is "node_modules". It is a bit directory and will increase your docker image dramatically.
