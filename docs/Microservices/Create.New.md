# Creating a new Microservice

1. Create a new GIT repository named `{my-solution}.{my-service}`
1. In Visual Studio, click to create a new project with the template ***M# ASP.NET Core - MVC Microservice***.
   - Choose a brief short name to only specify the role of that service.
     - Don't worry about the overall solution.
     - For example if you're creating the **Calendar** microservice for the **Lorem** solution for **Ipsum** company, just name the new project *Calendar".
   - Choose *Sql Server* and click **Create**

## Website\Properties\LaunchSettings.json

1. Select a port number that is unique in your overall solution
2. Set **applicationUrl** to *http://localhost:N* where **N** is the port number you selected.

## ..\BigPicture\Services.json

1. Add an entry to this file for the new service, similar to the other ones.

## Production environment: AWS
If using AWS for your production environment, use the following steps.

### AWS setup
You can set up the environment using the following tool:
`C:\Projects\Geeks.MS\BigPicture\PrepareServiceScript`


1. Edit `Settings.json` and set the *name* and *path* settings.
1. Go to `bin\Debug\netcoreapp2.1\`
1. Run `dotnet PrepareServiceScript.dll'
1. If it runs successfuly, you will see a file pop up named `@Instructions.txt`
   * Apply the specified changes. 


#### Blob Storage (optional)
Do the following steps only if your service needs file storage.

1. In AWS, create a new bucket named `{my-solution}.{my-service}`
1. In `appSettings.Production.json` set the value of `Blob:S3:Bucket` to the that name also.
1. Edit the `Runtime Role` that you created before. Add another inline policy to access the created bucket:
```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "s3:GetObject",
                "s3:ListBucket",
                "s3:DeleteObject",
                "s3:PutObject"
            ],
            "Resource": [
                "arn:aws:s3:::{my-solution}.{my-service}",
                "arn:aws:s3:::{my-solution}.{my-service}/*"
            ]
        }
    ]
}
```


