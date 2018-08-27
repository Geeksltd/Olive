# TODO: Remove the [old file and merge with this](Old-Jenkinsfile.md) and also [this one](Example-build.bat.md).
---

# Jenkinsfile for M# ASP.NET apps

The [Jenkins.md](Jenkins.md) describes the build process in details. This document includes the Jenkinsfile steps for building and automating a `ASP.Net MVC 5` and `Web Forms` projects used by the earlier versions of M#.

> Configuring the Jenkinsfile as below doesn't require the Build.bat file in the repository.

## Variables

| Variable  | Value to set |
| ------------- | ------------- |
| `GIT_REPO_SSH` | The SSH address of the git repository. |
| `GIT_BRANCH` | The name of the branch that will be used to build and deploy the application. |
| `GIT_CREDENTIALS_ID` | The ID of the SSH credentials record in Jenkins. The details will be provided below. |
| `WEBSITE_DLL_NAME` | The name of the website compiliation output. |
| `DOCKER_REPO_NAME`  | The name of the container repo on the container registry, where the docker images will be stored. |
| `DOCKER_REG_URL` | If using Docker Hub, set to "". Otherwise (e.g. if using AWS ECR, the url of the AWS container registry. |
| `K8S_SSH_SERVER`  | The url of the cluster. Can be found in the kubernetes config file in ~/.kube/.config  |
| `K8S_DEPLOYMENT` | To connect to Kubernetes you need to extract the certificate information of the user credentials, which is stored in `~/.kube/config`. |

## Credentials

The following should be added as `text credentials` in Jenkins.

| Variable  | Description |
| ------------- | ------------- |
| `DOCKER_REG_CREDENTIALS_ID` | Whether you use AWS ECR, or Docker Hub, create a Jenkins username/pass credentials and store its ID here. |
| `K8S_CERT_AUTH_CREDENTIALS_ID` |  The value of `certificate-authority-data` key in the config file. |
| `K8S_CLIENT_CERT_CREDENTIALS_ID` |  The value of `client-certificate-data` key in the config file. |
| `K8S_CLIENT_KEY_CREDENTIALS_ID` | The value of the `client-key-data` key in the config file. |


## Placeholders
Replace the following placeholders in the jenkinsfile with the correct values for your project:

| Placeholder  | Value to use |
| ------------- | ------------- |
| `#CONNECTION_STRING_CREDENTIALS_ID#` | The ID of the credentials record created for the connectionstring. You can store connectionstrings as a text credentials record in Jenkins. |


## The Jenkinsfile

```javascript
pipeline 
{
    environment 
    {   
        WEBSITE_DLL_NAME = "..."
        GIT_REPO_SSH = "..."
        GIT_CREDENTIALS_ID = "..."
        GIT_BRANCH = "..."
        
        DOCKER_REPO_NAME = "..."
        DOCKER_REG_URL = "..."
        DOCKER_REG_CREDENTIALS_ID = "..."                
        DOCKER_IMAGE = "${DOCKER_REPO_NAME}:v_${BUILD_NUMBER}" 

        K8S_SSH_SERVER = "..."
        K8S_DEPLOYMENT = ".\\DevOps\\Kubernetes\\Deployment.yaml"        
        K8S_DEPLOYMENT1 = "DevOps/Kubernetes/Deployment.yaml"
        
        K8S_CERT_AUTH_CREDENTIALS_ID = "..."
        K8S_CLIENT_CERT_CREDENTIALS_ID = "..."
        K8S_CLIENT_KEY_CREDENTIALS_ID = "..."
        
        PATH = "C:\\Nuget;C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\BuildTools\\MSBuild\\15.0\\Bin\\;$PATH"
        CONNECTION_STRING = credentials('#CONNECTION_STRING_CREDENTIALS_ID#');
    }
    agent any
    stages
        {   
            stage('Prepare the server') { steps { dir("\\DevOps\\Jenkins") { bat 'PrepairServer.bat' } } }
            
            stage('Delete prev build folder') { steps { script { deleteDir() } } }
                        
            stage('Git clone sources') { steps { script {
                 git branch: GIT_BRANCH, credentialsId: GIT_CREDENTIALS_ID, url: GIT_REPO_SSH
            }}}                       
           
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

            stage('Update bower, scripts, css') { steps { dir("Website") {
                script { bat 'bower install' }
                script 
                {                    
                    bat 'npm install gulp'
                    bat 'gulp less-to-css'                          
                    bat 'gulp fonts'
                }
             }}}
             
            stage('Compile source') { steps  {
                    script { dir("\\MsharpBuild") { bat 'msbuild' } }
                    script { dir("\\MsharpBuild\\Msharp") { bat 'Msharp.DSL.exe /build /model %workspace%' } }
                    script { dir("\\MsharpBuild\\Msharp") { bat 'Msharp.exe /build /model %workspace%' } }
                    script { dir("\\MsharpBuild\\Msharp") { bat 'Msharp.exe /build /ui %workspace%' } }                
            }}
            
            stage('Publish website') { steps { script { dir("Website") { bat 'MSBuild' }}}}
            
            stage('Verify build') { steps { script { dir("Website") {
                 bat '''if exist "bin\\%WEBSITE_DLL_NAME%.dll" (
                            echo Build Succeeded.
                        ) else (
                            echo Build Failed.
                            exit /b %errorlevel%
                        )'''                        
            }}}}
            
          
            stage('Publish as docker image') { steps { script {                            
                docker.withRegistry(DOCKER_REG_URL, DOCKER_REG_CREDENTIALS_ID)
                { docker.build(DOCKER_IMAGE).push(); }
                            
                // Using AWS ECR? Wrap the above code in the following.
                withAWS(credentials:DOCKER_REG_CREDENTIALS_ID)
                {
                    // Workaround (as the ECR Jenkins plugin is faulty):
                    sh("eval \$(aws ecr get-login --no-include-email --region eu-west-1 | sed 's|https://||')")
                    ###                     
                }
            }}}
      
            stage('Update K8s deployment file') { steps { script  {
                 sh ''' sed "s#%DOCKER_IMAGE%#${DOCKER_IMAGE}#g; s#%BUILD_VERSION%#v_${BUILD_NUMBER}#g" < $K8S_DEPLOYMENT > $K8S_DEPLOYMENT '''
            }}}
            
            stage('Deploy to cluster') { steps { script {                    
                 withCredentials([
                     string(credentialsId: K8S_CERT_AUTH_CREDENTIALS_ID, variable: 'K8S_CERT_AUTH'),
                     string(credentialsId: K8S_CLIENT_CERT_CREDENTIALS_ID, variable: 'K8S_CLIENT_CERT'),
                     string(credentialsId: K8S_CLIENT_KEY_CREDENTIALS_ID, variable: 'K8S_CLIENT_KEY')]) 
                 {                        
                      kubernetesDeploy(
                          credentialsType: 'Text',
                          textCredentials: [
                                    serverUrl: K8S_SSH_SERVER,
                                    certificateAuthorityData: K8S_CERT_AUTH,
                                    clientCertificateData: K8S_CLIENT_CERT,
                                    clientKeyData: K8S_CLIENT_KEY,
                          ],
                          configs: K8S_DEPLOYMENT1,
                          enableConfigSubstitution: true,
                      )
                  }
            }}}
    }
    post
    {
        always
        {
            // Remove the Docker image
            sh "docker rmi $IMAGE | true"
        }
    }
}
```

### Stage descriptions

| Stage  | Notes |
| ------------- | ------------- |
| `Prepare the server` | Runs `PrepairServer.bat` to install all the tools required to biuld the application. Tip: Dockerising the Jenkins server will eliminate this stage from the build process and shorten it. |
| `Update settings` | Populates dynamic placeholders (application version number, connection strings etc) in the source code. |
| `Publish website` | Required only for MVC projects. For WebForms projects it can be removed. |
| `Verify build` | Verifies if the `website dll` was successfully created. |
| `Publish as docker image` | Builds and pushes the docker image to the docker repository. |
| `Update K8s deployment file` | Inject the parameters (e.g. build version, the new docker image reference...) into the Kubernetes deployment file. (used to update the Kubernetes cluster). |


### Accessing Git via SSH
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

#### Tip: Efficient Docker images
Just like a git repository, you should avoid adding unnecessary files to your docker image. Copying `only` the files you require to `run the application` reduces the size of the docker image and make it more efficient and faster to pull and run it.

To avoid copying everything, you can use `.dockerignore` files. It has the same pattern as `.gitignore`. One of the most import folders you need to ignore is `node_modules` which is too large large.
