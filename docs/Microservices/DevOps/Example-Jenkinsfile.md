# Example of a Jenkinsfile

Below is an example of a Jenkinsfile. Comments have been added to describe what each section is and does.

## TODO: 
- Utilize some of the notification plugins Jenkins has to be able to give some feedback, whether a failure or a success, about the build process.
- Add a manual confirmation process right before the final step of the deployment process to avoid accidential live deployments?


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
            /// *** Only if building linux docker images ***
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
            
	    /// *** Only if building linux docker images ***
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
            /// *** Only if building linux docker images ***
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
            stage('Docker build (on linux)') /// *** Update the stage name to match the build environment ***
            {
                steps 
                {
                    node('LinuxDocker')/// *** Add this line Only if building linux docker images ***
                    {/// *** Add this line Only if building linux docker images ***                        
                        script 
                            {   
                                sh "docker build -t ${IMAGE_NAME} ."
                            }
                    }/// *** Add this line Only if building linux docker images ***
                }
            }
            
            /// This stage uses the Jenkins AWS Credentials plugin to login using the aws credentials that has write
            /// access to `AWS ECR (Amazon Elastic Container Registry)`. Once logged in, it uses the docker plugin to push the docker image created in the 
            /// previous stage to the ECR repository.
            stage('Push to ECR (on linux)') /// *** Update the stage name to match the build environment ***
            {
                steps 
                {
                    node('LinuxDocker')/// *** Add this line Only if building linux docker images ***
                    {        /// *** Add this line Only if building linux docker images ***
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
                    } /// *** Add this line Only if building linux docker images ***
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
