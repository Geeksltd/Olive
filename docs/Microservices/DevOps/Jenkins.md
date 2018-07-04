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

Below is an example of a Jenkinsfile:

```groovy
pipeline 
{
	parameters 
	{
		string(name: "authenticationDomain" , defaultValue : 'app.geeks.ltd')				
		string(name: "ecrAddress", defaultValue: '669486994886.dkr.ecr.eu-west-1.amazonaws.com')		
		string(name: "region", defaultValue: 'eu-west-1')						
		string(name: "k8sSSHServer" , defaultValue : 'https://api.app.geeks.ltd')																			
		string(name: "privateRepositoryAddress" , defaultValue : 'http://nuget.geeksms.uat.co/nuget')				
	}
    environment 
    {        
        IMAGE = 'geeksms-hub'		
		REGION_NAME = "${params.region}"
		ECR_URL = "https://${params.ecrAddress}"
		ECR_CRED = credentials('ECRCRED')
		BUILD_VERSION="v_${BUILD_NUMBER}"
		ECR_IMAGE_URL = "${params.ecrAddress}/${IMAGE}:${BUILD_VERSION}"        		
        AWS_CREDENTIALS_ID = 'JenkinsECRAWSCredentials'
		GIT_REPOSITORY = 'https://gitlab.com/Geeks.Microservices/accesshub.git'
		GIT_CREDENTIALS_ID = '1ef3615c-8221-4d33-af6d-91b203d60c75'
		BUILD_VERION_NUMBER = '0'				
		K8S_SSH_SERVER = "${params.k8sSSHServer}"
		K8S_DEPLOYMENT_TEMPLATE = ".\\DevOps\\Kubernetes\\Deployment.yaml"
		K8S_LATEST_DEPLOYMENT_FILE = ".\\DevOps\\Kubernetes\\Deployment${BUILD_VERSION}.yaml"				
		K8S_LATEST_CONFIG_FILE = "DevOps/Kubernetes/Deployment${BUILD_VERSION}.yaml"						
		PRIVATE_REPOSITORY_ADDRESS = "${params.privateRepositoryAddress}"
		PRIVATE_REPOSITORY_ADDRESS_NAME = "GeeksMS"
		AUTHENTICATION_DOMAIN = "${params.authenticationDomain}"
		HUB_SERVICE_URL = "https://hub.app.geeks.ltd"
    }
    agent any
    stages{    
			
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
            stage('Docker build (on linux)')
            {
				steps 
                {
                    node('LinuxDocker')
                    {                        
                        script 
                            {   
								sh "docker build -t ${IMAGE} ."
                            }
                    }
                }
            }
    
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
            // make sure that the Docker image is removed
            sh "docker rmi $IMAGE | true"
        }
    }
}


```

## Build Steps

### Msharp
If you have worked with M# or reviewed the link provided above you should know how it works. If you don't however, below is a short summary of how Msharp generates a project.

Each Msharp application has four projects. Two, `Model` and `UI`, used to define the metadata and structured used by MSharp to generate the application. The other two, `Domain` and `Website`, generated by Msharp using the metadata defined in Model and UI. After defining the application structure in Model and UI, you first need to build them using dotnet CLI, or in visual studio. There is a post-build-event defined in those two projects that run after they are built (duh!), which genrate the Domain and Website projects; However, there is a build sequence, provided below, you need to follow to generate the application property. 
* Model
* Domain
* UI
* Website

Because the UI project has to know what entities has been defined in the application it has a dependency to Domain. Once the Domain project is built, any changes to the entity structure - a new entity, new properties etc - made in Model will be visible to UI. The build process in Jenkins obviously has to follow the same order. Below are the steps taken to build and deploy an MSharp microservice in Jenkins

### step 1






## Build Servers
### Preparing the server
### TODO 
...
