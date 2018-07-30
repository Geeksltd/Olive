# Creating a new Microservice

1. Create a new GIT repository named `{my-solution}.{my-service}`
1. In Visual Studio, click to create a new project with the template ***M# ASP.NET Core - MVC Microservice***.
   - Choose a brief short name to only specify the role of that service.
     - Don't worry about the overall solution.
     - For example if you're creating the **Calendar** microservice for the **Lorem** solution for **Ipsum** company, just name the new project *Calendar".
   - Choose *Sql Server* and click **Create**

## Production environment: AWS
If using AWS for your production environment, do the following steps in AWS console:
#### 1. Create a new secret.
   - Choose `Other type of secrets` as the secret type.
   - Choose `Plaintext` and paste the following: `{ "ConnectionStrings": { "Default": "..." } }`
   - Name the secret `{my-solution}/{my-service}`   


#### 2. Create a Runtime Role 
1. Under IAM, [create a Role](https://console.aws.amazon.com/iam/home?region=eu-west-1#/roles)
1. Choose 'AWS Service' and then `EC2`
1. Grant applicable permissions.
   - If the service has a UI, add your authentication policy, e.g. `KMS_GeeksMS-Authentication_DecryptDataKey`
   - Add an inline policy for accessing the secret you created earlier:
```
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Sid": "VisualEditor0",
            "Effect": "Allow",
            "Action": "secretsmanager:GetSecretValue",
            "Resource": "{YOUR-Secret-URI}"
        }
    ]
}
```
1. Give it a name as `{my-service}Runtime`
      

## Application secrets
1. In AWS, 
2. In `appSettings.Production.json` set the value of `Aws:Secrets:Id` to `{my-solution}/{my-service}`.

## Blob storage
Do the following only if your service needs file storage.
1. If your service needs file storage, in AWS S3, create

## Website\appSettings.json

1. Open appSettings.json file
2. Set *"Authentication:CookieDataProtectorKey"* to the secret value that you use in your Auth service.



## Website\Properties\LaunchSettings.json

1. Select a port number that is unique in your overall solution
2. Set **applicationUrl** to *http://localhost:N* where **N** is the port number you selected.

## ..\BigPicture\Services.json

1. Add an entry to this file for the new service, similar to the other ones.
