# Olive DevOps Automation Service

This page describes the concept of a DevOps Service in Olive Framework. It also briefly covers the Automatic Nuget update and how to create a new microservice.

DevOps Service is a container for all related microservices that live in separate Visual Studio solution, to see the big picture in one view.

When you first open DevOps Service, the DevOps Service appears, and from there, you should point it to the root folder of the solution, In the root folder, create a subfolder named `BigPicture`
Inside it, add a file named `Services.json`

```json
{
    "Solution": {
        "ShortName": "geeksms",
        "FullName": "Geeks Operating System",		
        "Nuget": {
            "Url": "http://nuget.geeksms.uat.co/nuget",
            "ApiKey": "..."			
        },
		"CIServer": {
		    "Type": "Jenkins",
		    "Url": "http://jenkins.app.geeks.ltd/"
		},
		"Production": {
		    "Domain": "geeks.ltd"
		}
    },
    "Services": {
        "Hub": {
            "LiveUrl": "https://hub.app.geeks.ltd",
            "UatUrl": ""
        },
        "People": {
            "LiveUrl": "https://people.app.geeks.ltd",
            "UatUrl": ""
        }, 
        ...
}
```
it will then go through all sub-folders to determine if it's an Olive microservice. If it is, then it will generate a row on the UI.

![screenshot](Resources/Screenshot.JPG)
 
- The Status column is where you see the current state of the service and whether or not it's running locally. 
   - Green means it's Running, and red means it's off.
   - To start or stop service locally, you can press this button.
- The port value is read from the Website\Properties\LaunchSettings.json file. 
- If the service is running, the chrome icon will be enabled. it is a shortcut to launch a browser to view that service.
  - All services will be accessible via Hub.
- The *Git* column shows you how many new commits from other developers exist on the repository which you haven't pulled yet.
  - If there is no conflict, When you click on it, it will automatically pull the latest changes. 
- The Nuget column shows the number of outdated NuGet packages in that service.
  - You can click to update them from here automatically.
- The Visual Studio icon will open that service in a new Visual Studio.
  - But if it's already open, it will just bring the VS window to the top.
The *folder* icon will open a new explorer window to the source of that service.
The *gear* icon will compile that service.
The *debug* column will show you a log of events and errors for that service.
## Automatic Nuget update
In the main menu, you have the `Nuget` item with two options:
- *Update all*: It will update NuGet packages for all services in the list.
- *Auto update*: It will periodically check for NuGet updates and automatically update the packages.
## Create a microservice
To create a new microservice, choose `File` > `New Microservice`  on the menu bar. It will open a dialog that has options to name your service and set the repository for the new service.
Click `Create` to create a solution in the root folder of your project.
