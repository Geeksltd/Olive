The [Jenkins.md](Olive/docs/Microservices/DevOps/Jenkins.md) describes the build process in details. This document includes the Jenkinsfile steps for building and automating an ASP.net MVC project.

*** Configuring the Jenkinsfile as below doesn't require the Build.bat file in the repository. ***

```groovy
pipeline 
{
    environment 
    {   
        BUILD_VERSION = "v_${BUILD_NUMBER}"
        IMAGE = "#DOCKER_REPOSITORY_NAME#:${BUILD_VERSION}"     
        WEBSITE_PATH="Website"
        GIT_REPOSITORY = "#GIT_REPOSITORY_SSH_ADDRESS#"                         
        K8S_SSH_SERVER = "#KUBERNETES_CLUSTER_URL#"
        K8S_DEPLOYMENT_TEMPLATE = ".\\DevOps\\Kubernetes\\Deployment.yaml"
        K8S_LATEST_DEPLOYMENT_FILE = ".\\DevOps\\Kubernetes\\Deployment${BUILD_VERSION}.yaml"               
        K8S_LATEST_CONFIG_FILE = "DevOps/Kubernetes/Deployment${BUILD_VERSION}.yaml"
        PATH = "C:\\Nuget;C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\BuildTools\\MSBuild\\15.0\\Bin\\;$PATH"
        CONNECTION_STRING = credentials('#CONNECTION_STRING_CREDENTIALS_ID#');      
    }
    agent any
    stages
        {   
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
                        
            stage('Clone sources') 
            {
                steps
                    {
                        script
                            {                       
                                git branch:"#BRANCH_NAME#", credentialsId: "#GIT_CREDENTIALS_ID#", url: GIT_REPOSITORY
                            }
                    }               
            }
            
            stage('Prepare the server')
            {
                steps 
                {   
                    dir("\\DevOps\\Jenkins")
                    {
            // This file installs all the tolls required to biuld the application. Dockerising the Jenkins server will eliminate this stage from the build process and shorten it.                      
                        bat 'PrepairServer.bat'  
                    }
                                                
                }
            }
        // This stage populates dynamic placeholders (application version number, connection strings etc) in the source code. 
        stage('Update settings') // This sate
            {
                steps
                {
                    script
                        {   
                            dir(WEBSITE_PATH)       
                            {                           
                 sh """ sed -i "s/%APP_VERSION%/${BUILD_NUMBER}/g;s/%CONNECTION_STRING%/${CONNECTION_STRING}/g;" web.config """                                                 
                                  }
                        }
                }
            }
            
        stage('Delete the GCOP References')
            {
                steps 
                {
                    script 
                        {
                           bat 'nuget sources'
                           powershell 'DevOps/DeleteGCopReferences.ps1'
                           bat 'for /r %%i in (.\\*.config) do (type %%i | find /v "GCop" > %%i_temp && move /y %%i_temp %%i)'
                        }
                        
                            
                }
            } 
            
            stage('Restore nuget packages')
            {
                steps 
                {
                    bat 'nuget restore'
                    
                }
            }
            
            stage('Build the source code')
            {
                steps 
                {
                    script 
                        {
                            dir("\\MsharpBuild")
                            {
                                bat 'msbuild'
                            }                           
                        }   
                    script
                        {
                            dir("\\MsharpBuild\\Msharp")
                            {
                                bat 'Msharp.DSL.exe /build /model %workspace%'                                                              
                            }
                        }
                    script
                        {
                            dir("\\MsharpBuild\\Msharp")
                            {
                                bat 'Msharp.exe /build /model %workspace%'      
                            }
                        }
                    script
                        {
                            dir("\\MsharpBuild\\Msharp")
                            {
                                bat 'Msharp.exe /build /ui %workspace%'     
                            }
                        }                       
                        
                }
            }          
            stage('Prepare runtime resources (bower components, scripts, css)')
            {
                steps 
                {
                    dir(WEBSITE_PATH)
                    {
                        script 
                        {
                    
                            bat 'bower install'                                             
                        }
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

                steps 
                {
                    script
                        {
                            dir(WEBSITE_PATH)
                            {
                                bat 'MSBuild'                       
                            }
                        }                       
                        
                }
            }
            // This stage checks to see if the build process has successfully created the webiste dll. It checks the website binary folder path for [#WEBSITE.DDL.NAME#].dll
            stage('Check build')
            {
                steps 
                {
                    script 
                    {
                        dir(WEBSITE_PATH)
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
                        
                                docker.withRegistry(#ECR_URL#, "CONTAINER_REPOSITORY_CREDENTIALS_ID") 
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
