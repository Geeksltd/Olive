# Creating a new Microservice

1. Create a new GIT repository named `{my-solution}.{my-service}`
1. In Visual Studio, click to create a new project with the template ***M# ASP.NET Core - MVC Microservice***.
   - Set the project name as `{my-solution}.{my-service}`. For example, if you're creating the **Calendar** microservice for the **Lorem** solution name the new project "*Lorem.Calendar*".

## ..\BigPicture\Services.json

1. Add an entry to this file for the new service, similar to the other ones.

## Production environment: AWS
If using AWS for your production environment, use the following steps.

You can set up the environment using the following tool:
`C:\Projects\Geeks.MS\BigPicture\PrepareServiceScript`

1. Edit `Settings.json` and set the *name* and *path* settings.
1. Go to `bin\Debug\netcoreapp2.1\`
1. Run `dotnet PrepareServiceScript.dll'
1. If it runs successfuly, you will see a file pop up named `@Instructions.txt`
   * Apply the specified changes. 


