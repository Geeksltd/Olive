## Introduction

The MC# application repositories only contain MSharp metadata files. The CI/CD process pulls the metadata and builds the application by MSharp, which currently requires .NetFramework on Windows to run. The production environment runs on Linux AWS EC2 instances on Docker and managed by Kubernetes. In order to run a service on the production we have to generate Linux Docker images, which obviously require the Linux Docker engine. 
Running Linux Docker engine on Windows is achieved by running a Linux VM. Unfortunately since AWS EC2 instances are virtual machines, it is not possible to run another VM on top of them (at least not easily). 
To have access to both Windows and Linux environments on the build server we need to add a Linux worker node to Jenkins. Fortunately Jenkins pipeline supports running stages on different nodes which enables us to build the application on a Windows node and use the generated artefacts on a Linux node and generate the Linux docker image.

Preparing The Build Cluster
Below is the instruction for preparing the Windows master and Linux worker node. The master node is being used as a worker node, so we don’t need to add any more EC2 instances.


### Master
First create a EC2 instance (Windows) on AWS. Make sure you store the keypair somewhere safe. 
During the launch process make sure:
Create a new VPC for the build cluster.
Create a new security group to use for the build nodes.
The security group has to open RDP connection to the company’s static IP address.
Create a new volume to store the Jenkins home directory on for the master node.


After the instance is launched and running, RDP to it and install the following.
- Git (with default settings)
   - Run the following commands:
     - mklink "C:\Program Files\Git\bin\nohup.exe" "C:\Program Files\git\usr\bin\nohup.exe"
     - mklink "C:\Program Files\Git\bin\msys-2.0.dll" "C:\Program Files\git\usr\bin\msys-2.0.dll"
     - mklink "C:\Program Files\Git\bin\msys-iconv-2.dll" "C:\Program Files\git\usr\bin\msys-iconv-2.dll"
     - mklink "C:\Program Files\Git\bin\msys-intl-8.dll" "C:\Program Files\git\usr\bin\msys-intl-8.dll"
     - mklink "C:\Program Files\Git\cmd\nohup.exe" "C:\Program Files\git\usr\bin\nohup.exe"
     - mklink "C:\Program Files\Git\cmd\msys-2.0.dll" "C:\Program Files\git\usr\bin\msys-2.0.dll"
     - mklink "C:\Program Files\Git\cmd\msys-iconv-2.dll" "C:\Program Files\git\usr\bin\msys-iconv-2.dll"
     - mklink "C:\Program Files\Git\cmd\msys-intl-8.dll" "C:\Program Files\git\usr\bin\msys-intl-8.dll"
     - mklink "C:\Program Files\Git\cmd\sh.exe" "C:\Program Files\git\usr\bin\sh.exe"
   - Add C:\Program Files\Git\usr\bin to the PATH variable  
- Nuget
- Install Hyper-v
- Jenkins 
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
