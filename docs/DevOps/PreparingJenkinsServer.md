# Preparing Jenkins Server

The MC# application repositories only contain MSharp metadata files. The CI/CD process pulls the metadata and builds the application by MSharp, which currently requires .NetFramework on Windows to run. 

## Production Linux Runtime
If you choose, or even have to, run your application in Linux servers there are a few more steps to be taken. In order to run a service on Linux servers we have to generate Linux Docker images. Generating linux docker images require the Linux Docker engine on the build server. 
Running Linux Docker engine on Windows is achieved by running a Linux VM. Unfortunately since AWS EC2 instances are virtual machines, it is not possible to run another VM on top of them (at least not easily). 
To have access to both Windows and Linux environments on the build server we need to add a Linux worker node to Jenkins. Fortunately Jenkins pipeline supports running stages on different nodes which enables us to build the application on a Windows node and use the generated artefacts on a Linux node and generate the Linux docker image.

If the production envirnoment only has windows instances you can ignore the parts related to Linux worker nodes. Your Windows master node works as a worker node by default so you don't have to set up a separate worker node.

Preparing The Build Cluster
Below is the instruction for preparing the Windows master and Linux worker node. The master node is being used as a worker node, so we don’t need to add any more EC2 instances.

This document is based on AWS but the same instructions applys to other cloud providers such as Azure and Google Cloud.


## Master

#### AWS
First create a EC2 instance (Windows) on AWS. Make sure you store the keypair somewhere safe. 
During the launch process make sure:
Create a new VPC for the build cluster.
Create a new security group to use for the build nodes.
The security group has to open RDP connection to the company’s static IP address.
Create a new volume to store the Jenkins home directory on for the master node.

#### Azure
// TODO


After the instance is launched and running, RDP to it and install the following.
- [Jenkins](https://github.com/Geeksltd/Olive/blob/master/docs/DevOps/Jenkins.md)
   - You can download and install Jenkins from [here](https://jenkins.io/download/)
- Git (with default settings)
   - Run the following commands:
   ```
     - mklink "C:\Program Files\Git\bin\nohup.exe" "C:\Program Files\git\usr\bin\nohup.exe"
     - mklink "C:\Program Files\Git\bin\msys-2.0.dll" "C:\Program Files\git\usr\bin\msys-2.0.dll"
     - mklink "C:\Program Files\Git\bin\msys-iconv-2.dll" "C:\Program Files\git\usr\bin\msys-iconv-2.dll"
     - mklink "C:\Program Files\Git\bin\msys-intl-8.dll" "C:\Program Files\git\usr\bin\msys-intl-8.dll"
     - mklink "C:\Program Files\Git\cmd\nohup.exe" "C:\Program Files\git\usr\bin\nohup.exe"
     - mklink "C:\Program Files\Git\cmd\msys-2.0.dll" "C:\Program Files\git\usr\bin\msys-2.0.dll"
     - mklink "C:\Program Files\Git\cmd\msys-iconv-2.dll" "C:\Program Files\git\usr\bin\msys-iconv-2.dll"
     - mklink "C:\Program Files\Git\cmd\msys-intl-8.dll" "C:\Program Files\git\usr\bin\msys-intl-8.dll"
     - mklink "C:\Program Files\Git\cmd\sh.exe" "C:\Program Files\git\usr\bin\sh.exe"
   ```  
   - Add C:\Program Files\Git\usr\bin to the PATH variable by running  ``` set PATH=%PATH%;C:\Program Files\Git\usr\bin ```
- Make sure the correct version of your framework SDK (.net framework or .net core) is installed.
- For .Net Framework
   - Make sure you have installed MSBuild and added the path to the msbuild.exe to the PATH environment variable.
   - When installing MSBuild, make sure you install "Nuget targets and build tasks". 
      - While the Visual Studio Installer is still running, go to the "Individual Components" tab
      - Tick the "NuGet package manager" check-box that is under "Code tools" option.
      - Click Install to install it.
- Nuget
   - Make sure it is accessible in cmd by typing nuget in a command prompt. If it is not, run  ``` set PATH=%PATH%;[PATH TO THE NUGET EXE] ```
- Install Hyper-v
- Jenkins Set Up
   - Restoring a backed up version:
     - Copy the backup Jenkins folder to the destination folder
     - Navigate to the Jenkins folder
     - Run cmd
     - Run : jenkins.exe install
     - Run : jenkins.exe start
   - Accessing Jenkins via a custom URL
     - Install IIS and the URL Rewrite module.
     - Change the Jenkins configuration to bind the Jenkins service to 127.0.0.1. This can be done by adding --httpListenAddress=127.0.0.1 to the service  > arguments section of the jenkins.xml file on the jenkins home directory.
     - Then you need to create a website (or you can use the default website) and copy the below configuration to the web.config 
```
<system.webServer>
        <urlCompression doStaticCompression="false" />
        <rewrite>
            <rules>
                <rule name="ReverseProxyInboundRule1" stopProcessing="true">
                    <match url="(.*)" />
                    <action type="Rewrite" url="http://127.0.0.1:8080/{R:1}" />
                </rule>
            </rules>
        </rewrite>
    </system.webServer>
```
TODO : Enable SSL for Jenkins


## Plugins
The current build script uses some 3rd party APIs such as AWS, Docker and Kubernetes as well as some custom functionalities in Jenkins implemented as plugins which we have to install in Jenkins. 

Below is a list of all plugins :

```
JavaScript GUI Lib: ACE Editor bundle plugin (ace-editor): 1.1
Amazon ECR plugin (amazon-ecr): 1.6
Ant Plugin (ant): 1.8
OWASP Markup Formatter Plugin (antisamy-markup-formatter): 1.5
Apache HttpComponents Client 4.x API Plugin (apache-httpcomponents-client-4-api): 4.5.3-2.1
Authentication Tokens API Plugin (authentication-tokens): 1.3
CloudBees Amazon Web Services Credentials Plugin (aws-credentials): 1.23
Amazon Web Services SDK (aws-java-sdk): 1.11.264
Azure Commons Plugin (azure-commons): 0.2.4
Azure Credentials (azure-credentials): 1.6.0
bouncycastle API Plugin (bouncycastle-api): 2.16.2
Branch API Plugin (branch-api): 2.0.18
Build Timeout (build-timeout): 1.19
Folders Plugin (cloudbees-folder): 6.3
Command Agent Launcher Plugin (command-launcher): 1.2
Credentials Binding Plugin (credentials-binding): 1.14
Credentials Plugin (credentials): 2.1.16
Display URL API (display-url-api): 2.2.0
CloudBees Docker Build and Publish plugin (docker-build-publish): 1.3.2
Docker Commons Plugin (docker-commons): 1.11
Docker Pipeline (docker-workflow): 1.15
Durable Task Plugin (durable-task): 1.17
Email Extension Plugin (email-ext): 2.61
External Monitor Job Type Plugin (external-monitor-job): 1.7
Git client plugin (git-client): 2.7.1
GIT server Plugin (git-server): 1.7
Git plugin (git): 3.7.0
GitHub API Plugin (github-api): 1.90
GitHub Branch Source Plugin (github-branch-source): 2.3.2
GitHub plugin (github): 1.29.0
Gradle Plugin (gradle): 1.28
JavaScript GUI Lib: Handlebars bundle plugin (handlebars): 1.1.1
Jackson 2 API Plugin (jackson2-api): 2.8.10.1
JDK Tool Plugin (jdk-tool): 1.0
JavaScript GUI Lib: jQuery bundles (jQuery and jQuery UI) plugin (jquery-detached): 1.2.1
JSch dependency plugin (jsch): 0.1.54.1
JUnit Plugin (junit): 1.23
Kubernetes Continuous Deploy Plugin (kubernetes-cd): 0.1.4
Kubernetes Credentials Plugin (kubernetes-credentials): 0.3.0
LDAP Plugin (ldap): 1.19
Mailer Plugin (mailer): 1.20
MapDB API Plugin (mapdb-api): 1.0.9.0
Matrix Authorization Strategy Plugin (matrix-auth): 2.2
Matrix Project Plugin (matrix-project): 1.12
JavaScript GUI Lib: Moment.js bundle plugin (momentjs): 1.1.1
MSBuild Plugin (msbuild): 1.29
PAM Authentication plugin (pam-auth): 1.3
Pipeline: AWS Steps (pipeline-aws): 1.21
Pipeline: Build Step (pipeline-build-step): 2.7
Pipeline: GitHub Groovy Libraries (pipeline-github-lib): 1.0
Pipeline Graph Analysis Plugin (pipeline-graph-analysis): 1.6
Pipeline: Input Step (pipeline-input-step): 2.8
Pipeline: Milestone Step (pipeline-milestone-step): 1.3.1
Pipeline: Model API (pipeline-model-api): 1.2.7
Pipeline: Declarative Agent API (pipeline-model-declarative-agent): 1.1.1
Pipeline: Declarative (pipeline-model-definition): 1.2.7
Pipeline: Declarative Extension Points API (pipeline-model-extensions): 1.2.7
Pipeline: REST API Plugin (pipeline-rest-api): 2.9
Pipeline: Stage Step (pipeline-stage-step): 2.3
Pipeline: Stage Tags Metadata (pipeline-stage-tags-metadata): 1.2.7
Pipeline: Stage View Plugin (pipeline-stage-view): 2.9
Plain Credentials Plugin (plain-credentials): 1.4
PowerShell plugin (powershell): 1.3
Resource Disposer Plugin (resource-disposer): 0.8
SCM API Plugin (scm-api): 2.2.6
Script Security Plugin (script-security): 1.40
SSH Credentials Plugin (ssh-credentials): 1.13
SSH Slaves plugin (ssh-slaves): 1.25.1
Structs Plugin (structs): 1.10
Subversion Plug-in (subversion): 2.10.2
Timestamper (timestamper): 1.8.9
Token Macro Plugin (token-macro): 2.3
Windows Slaves Plugin (windows-slaves): 1.3.1
Pipeline (workflow-aggregator): 2.5
Pipeline: API (workflow-api): 2.25
Pipeline: Basic Steps (workflow-basic-steps): 2.6
Pipeline: Shared Groovy Libraries (workflow-cps-global-lib): 2.9
Pipeline: Groovy (workflow-cps): 2.43
Pipeline: Nodes and Processes (workflow-durable-task-step): 2.18
Pipeline: Job (workflow-job): 2.17
Pipeline: Multibranch (workflow-multibranch): 2.17
Pipeline: SCM Step (workflow-scm-step): 2.6
Pipeline: Step API (workflow-step-api): 2.14
Pipeline: Supporting APIs (workflow-support): 2.17
Workspace Cleanup Plugin (ws-cleanup): 0.34
```
To be able to install them you have to save the list in a file (i.e. plugins.txt) and follow the instruction [here](https://jenkins.io/doc/book/managing/plugins/#install-with-cli)
