# Docker support

We offer Docker support for Olive Microservice solution. Why? Because it's much easier to develop, run and maintain! It fixes the most common developer issue: "*It does not work in my machine*".  And so much other benefits that might come to your mind. If you have questions about Docker containers and Docker usage in microservice architecture, [HERE](http://www.zdnet.com/article/what-is-docker-and-why-is-it-so-darn-popular/) and [HERE](https://blogs.msdn.microsoft.com/dotnet/2017/08/02/microservices-and-docker-containers-architecture-patterns-and-development-guidance/) are good ones to get started.

## How it works

Easily! We put every microservice and other required services (Such as database engine) in separate Docker containers.

## Getting Docker ready

First you need to install Docker and Docker toolbox in your machine. You can get Docker Docker toolbox from [HERE](https://www.docker.com/get-docker). You can install them in Linux and Windows 10 (Anniversary update or later). But it can't be installed in Windows server 2016.

### Installation in Windows server 2016

Docker and container is supported in Windows server 2016. But just windows containers. Here's instructions to add Linux Docker support to Windows server 2016.

1- Open up PowerShell as administrator and run `Install-WindowsFeature hyper-v,containers` command, then restart the machine. Make sure that Hyper-V feature is enabled.

2-Swich to your Windows 10 computer, Download "Docker for Windows" : [https://docs.docker.com/docker-for-windows](https://docs.docker.com/docker-for-windows/)

3-Copy `C:\Program Files\Docker` from the Windows 10 machine to `C:\Program Files\Docker` on your Server 2016.

4- Switch to Windows Server 2016, then import these registry settings:
``` 
---START---
Windows Registry Editor Version 5.00

[HKEY_LOCAL_MACHINE\SOFTWARE\Docker Inc.]

[HKEY_LOCAL_MACHINE\SOFTWARE\Docker Inc.\Docker]

[HKEY_LOCAL_MACHINE\SOFTWARE\Docker Inc.\Docker\1.0]
"AppPath"="\"C:\Program Files\Docker\Docker\Docker for Windows.exe\""
"BinPath"="\"C:\Program Files\Docker\Docker\resources\bin\""

[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\EventLog\Application\DockerService]
"EventMessageFile"=hex(2):43,00,3a,00,5c,00,57,00,69,00,6e,00,64,00,6f,00,77,\ 00,73,00,5c,00,4d,00,69,00,63,00,72,00,6f,00,73,00,6f,00,66,00,74,00,2e,00,\ 4e,00,45,00,54,00,5c,00,46,00,72,00,61,00,6d,00,65,00,77,00,6f,00,72,00,6b,\ 00,36,00,34,00,5c,00,76,00,34,00,2e,00,30,00,2e,00,33,00,30,00,33,00,31,00,\ 39,00,5c,00,45,00,76,00,65,00,6e,00,74,00,4c,00,6f,00,67,00,4d,00,65,00,73,\ 00,73,00,61,00,67,00,65,00,73,00,2e,00,64,00,6c,00,6c,00,00,00

[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\com.docker.service] "Type"=dword:00000010 "Start"=dword:00000002 "ErrorControl"=dword:00000000 "ImagePath"=hex(2):22,00,43,00,3a,00,5c,00,50,00,72,00,6f,00,67,00,72,00,61,00,\ 6d,00,20,00,46,00,69,00,6c,00,65,00,73,00,5c,00,44,00,6f,00,63,00,6b,00,65,\ 00,72,00,5c,00,44,00,6f,00,63,00,6b,00,65,00,72,00,5c,00,63,00,6f,00,6d,00,\ 2e,00,64,00,6f,00,63,00,6b,00,65,00,72,00,2e,00,73,00,65,00,72,00,76,00,69,\ 00,63,00,65,00,22,00,00,00
"DisplayName"="Docker for Windows Service"
"ObjectName"="LocalSystem"
"Description"="Run Docker for Windows backend service"
---END---
```

5- Restart the computer and run the PowerShell script `'C:\Program Files\Docker\Docker\Resources\MobyLinux.ps1' -create`

6- Go to `C:\Program Files\Docker\` and run `Docker for Windows.exe`.

## Running dockerized microservices

Go to the solution, select `Docker Compose` as start-up project. Then press f5 and run the solution!
