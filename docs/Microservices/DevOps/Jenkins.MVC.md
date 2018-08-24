The [Jenkins.md](Olive/docs/Microservices/DevOps/Jenkins.md) describes the build process in details. This document includes the Jenkinsfile steps for building and automating an ASP.net MVC project.



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
		  CONNECTION_STRING = credentials('CONNECTION_STRING');		
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
								git branch:"#BRANCH_NAME#", credentialsId: "SSC_GIT_CREDENTIALS", url: GIT_REPOSITORY
							}
					}				
			}
			
			stage('Prepare the server')
            {
                steps 
                {	
                    dir("\\DevOps\\Jenkins")
                    {
                        bat 'PrepairServer.bat'								
                    }
                     							
                }
            }
				
			stage('Update settings') 
            {
                steps
                {
                    script
                        {	
    	                    dir(WEBSITE_PATH)		
	                        {                        	
								withCredentials([[$class: 'UsernamePasswordMultiBinding', credentialsId: 'SSC_DB_CREDENTIALS',usernameVariable: 'SSC_DB_USER', passwordVariable: 'SSC_DB_USER_PASSWORD']]) 
                                {
								    sh """ sed -i "s/%APP_VERSION%/${BUILD_NUMBER}/g;s/%SSC_DB_SERVER%/${SSC_DB_SERVER}/g;s/%SSC_DB_USER%/${SSC_DB_USER}/g;s/%SSC_DB_USER_PASSWORD%/$SSC_DB_USER_PASSWORD/g;" web.config """													
                                }
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
			stage('Create App_Data')
            {

                steps 
                {
					script
						{
							dir(WEBSITE_PATH)
							{
								bat 'mkdir App_Data'						
							}
						}						
						
                }
            }
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
			stage('Check build')
			{
				steps 
				{
					script 
					{
						dir(WEBSITE_PATH)
						{
							bat '''if exist "bin\\ShootingStarChase.Website.dll" (
								echo Build Succeeded.
								) else (
								echo Build Failed.
								exit /b %errorlevel%
								)'''						
						}
					}
				}
			}
		 	stage('Build and publish the docker images') 
		 	{
    			
      			steps 
      			{
      				script 
						{
	   						docker.withRegistry("","SSC_DOCKER_HUB_CREDENTIALS") 
    						{   
          						def img = docker.build(IMAGE);
          						img.push();          						        						
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
					
						withCredentials([string(credentialsId: 'SSC_CertificateAuthorityData', variable: 'K8S_CERTIFICATE_AUTHORITY_DATA'), string(credentialsId: 'SSC_ClientCertificateData', variable: 'K8s_CLIENT_AUTHORITY_DATA'), string(credentialsId: 'SSC_ClientKeyData', variable: 'K8s_CERTIFICATE_KEY_DATA')]) 
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
