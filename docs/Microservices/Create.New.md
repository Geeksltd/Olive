# Creating a new Microservice

1. Create a new GIT repository named `{my-solution}.{my-service}`
1. Download, build and run [Olive Microservice Explorer](https://github.com/Geeksltd/Olive.Microservice.Explorer/blob/master/README.md)
1. From `File` > `New Microservice` menu, [create a new service](https://github.com/Geeksltd/Olive.Microservice.Explorer/blob/master/README.md#new-microservice-creation)

## Production environment: AWS
If using AWS for your production environment, use the following steps.

You can set up the environment using the following tool:
`C:\Projects\Geeks.MS\BigPicture\PrepareServiceScript`

1. Edit `Settings.json` and set the *name* and *path* settings.
1. Go to `bin\Debug\netcoreapp2.1\`
1. Run `dotnet PrepareServiceScript.dll`
1. If it runs successfuly, you will see a file pop up named `@Instructions.txt`
   * Apply the specified changes. 


