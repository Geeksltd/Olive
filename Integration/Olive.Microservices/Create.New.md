# Creating a new Microservice
1. In Visual Studio, click to create a new project with the template ***M# ASP.NET Core - MVC Microservice***.
2. Choose a brief short name to only specify the role of that service. Don't worry about the overall solution. For example if you're creating the **Calendar** microservice for the **Lorem** solution for **Ipsum** company, just name the new project *Calendar".
3. Choose *Sql Server* and click **Create**

## Website\appSettings.json
1. Open appSettings.json file
5. Set *"Authentication:CookieDataProtectorKey"* to the secret value that you use in your Auth service.

## Website\Properties\LaunchSettings.json
1. Select a port number that is unique in your overall solution
2. Set **applicationUrl** to *http://localhost:N* where **N** is the port number you selected.

## ..\BigPicture\Services.json
1. Add an entry to this file for the new service, similar to the other ones.
