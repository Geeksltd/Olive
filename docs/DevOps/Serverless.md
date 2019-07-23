# Serverless deployment on AWS Lambda

## Deploying M# ASP.NET web app as a Serverless Application
### Understanding the roles
There are two security principal concepts here:
1. **Runtime role:** This is a role used at runtime by the lambda function. The application essentially runs under this role. It needs to have as little permissions as possible. For example if the function needs to download data from a certain S3 bucket, it should have access to that, and nothing more.
2. **Cloudformation role:** A role with the power to create AWS resources for creating the Lambda's required hardware. This role is passed on to the built-in AWS service `cloudformation.amazon.com` (called the Serverless Framework) so it can create the hardware resources on behalf of us.
3. **Deployment role:** A role with the power to deploy a specific lambda function.
4. **Developer user:** This is the person (usually a senior developer) who will have an AWS user. He/she will often be given temporary access to the Cloudformation and Deployment roles in order to execute the deployment. The purpose of this separation is that the security settings are defined only once, and then depending on who is executing it, access to those roles can be assigned without changing anything else.
### Step 1: Create an S3 bucket -> {my-function}-deployment
Leave all permissions as default.
### Step 2: Create a Lambda execution role -> {my-function}-runtime
Admin should create an IAM role called "{my-function}-runtime" with the following settings.
- The use case should be selected as Lambda.
- Grant access to the policy of **AWSLambdaBasicExecutionRole**.
- Add any additional permissions required by your function, for example to access S3, RDS, etc.
Grab the ARN of the role and add it to serverless.template under **Resources/AspNetCoreFunction/Role**.
### Step 3: Create an IAM Policy -> "{my-function}-deployer"
Admin should create a policy with the following settings, so it can create the necessary AWS resources. The use case of the role should be selected as CloudFormation.
```json
{
    "Version": "2012-10-17",
    "Statement": [
        { "Effect": "Allow", "Action": "cloudformation:*", "Resource": "arn:aws:cloudformation:{MyRegion}:{MyAccountId}:stack/{my-function}**" },
        { "Effect": "Allow", "Action": ["cloudformation:CreateChangeSet", "cloudformation:ValidateTemplate"], "Resource" : "*" },
        { "Effect": "Allow", "Action": "iam:PassRole", "Resource": "arn:aws:iam::{MyAccountId}:role/{my-function}-runtime" },
        { "Effect": "Allow", "Action":  "apigateway:*", "Resource": [ "arn:aws:apigateway:*::/restapis", "arn:aws:apigateway:*::/restapis/*" ] },      
        { "Effect": "Allow", "Action":  "lambda:*", "Resource":  "arn:aws:lambda:*:{MyAccountId}:function:{my-function}*" }
        { "Effect": "Allow", "Action":  "s3:*", "Resource": [ "arn:aws:s3:::{my-function}-deployment", "arn:aws:s3:::{my-function}-deployment/*" ] },       
    ]
}
```
#### Granting developer access
Admin user should create an IAM user for the developer in charge of deploying the Lambda app.
Then the deployer policy should be assigned to the user.
For the developer user, create a programmatic access key pair. 
The developer can then open CLI and log in to AWS using the following command:
```
$ aws configure set default.region us-west-1
$ aws configure set aws_access_key_id ...
$ aws configure set aws_secret_access_key ...
```
By doing this, AWS commands can now be executed under the user's security principal.
The above settings will be stored under "%UserProfile%\.aws".
### Create AWS Serverless Project
- Install AWS Toolkit for Visual Studio from [here](https://aws.amazon.com/visualstudio/).
- Create a new project using AWS Lambda blueprint. (File -> New Project -> AWS Lambda -> AWS Serverless Application (.NET Core)).
- On the Select Blueprint, choose ASP.NET Core Web App and then click Finish.
#### Configuring the project
Open **aws-lambda-tools-default.json** from root of your project and set its values like this:
```json
{
    "profile"     : "default",
    "region"      : "eu-west-1",
    "configuration" : "Release",
    "framework"     : "netcoreapp2.1",
    "template"      : "serverless.template",
    "s3-bucket"     : "{my-function}-deployment",
    "stack-name"    : "{my-function}"
}
```
Update the **serverless.template** file to the following.
```json
{
   "AWSTemplateFormatVersion" : "2010-09-09",
   "Transform" : "AWS::Serverless-2016-10-31",
   "Description" : "An AWS Serverless Application that uses the ASP.NET Core framework running in Amazon Lambda.",
   "Resources" : {
      "AspNetCoreFunction" : {
         "Type" : "AWS::Serverless::Function",
         "Properties": {
            "Handler": "WordUp.AWS.Lambda::WordUp.AWS.Lambda.LambdaEntryPoint::FunctionHandlerAsync",
            "Runtime": "dotnetcore2.1",            
            "MemorySize": 256,
            "Timeout": 30,
            "Role": "arn:aws:iam::{MyAccountId}:role/{my-function}-runtime",            
            "Events": {
               "ProxyResource": { "Type": "Api", "Properties": { "Path": "/{proxy+}", "Method": "ANY" } },
               "RootResource": { "Type": "Api", "Properties": { "Path": "/", "Method": "ANY" } }
            }
         }
      }
   },
   "Outputs" : {
      "ApiURL" : {
         "Description" : "API endpoint URL for live environment",
         "Value" : { "Fn::Sub" : "https://${ServerlessRestApi}.execute-api.${AWS::Region}.amazonaws.com/" }
      }
   }
}
```
#### Deploying to AWS Lambda
You should ensure that you have the lambda dotnet tool installed:
```
dotnet tool install --global Amazon.Lambda.Tools --version 3.2.3
```
Now you need to deploy your application into the AWS Lambda. Run a command prompt in the root of your project. And run this command:
```
dotnet lambda deploy-serverless
```
Congratulations! Your application is now Serverless! Just use the link in front of ApiURL to access to your application. 
### Assigning API Endpoint to a Custom Domain Name
#### Step 1: Requesting for an SSL Certificate
To assigning custom domain name to the API endpoint, First you need to request for an SSL certificate. 

Log on to AWS Console and then go to **Certificate Manager**. The click on **Request a certificate** and select **Request a public certificate**. In the next page Add domain name(s) that will use this certificate. 

At the next page select the validation method. Notice that for using **Email validation** you need to have access to domain default Emails like (admin@example.com or webmaster@example.com). After that wait for AWS to issue a new certificate. for Email validation you have to check above mentioned Email for verification link and for Domain verification you need to update CNAME on your DNS using the provided values by AWS.
#### Step 2: Creating a Custom Domain Name
Log on AWS Console and then go to **Amazon API Gateway**. In the left panel select **Custom Domain Names** and the click on **Create Custom Domain Name**. 

In **New Custom Domain Name**, Select **Http** as your endpoint protocol. In **Domain Name** put the exact domain name as the one you've put in your SSL certificate in Step 1. Select **Regional** as your **Endpoint Configuration** and for the **ACM Certificate**, Select the SSL certificate you've created in Step 1 and click **Save**. In the next section, select the stack name of your api which usually is identical with your function name. Notice that **stage** should be identical with your published serverless application. After that click Save. 
#### Step 3: Updating DNS Server Record
Log on to to your DNS Server and then Update the CNAME for your domain with **Target Domain Name** and **Hosted Zone ID**. It usually take few hour to the web routers address resolver table to get uptated. After that you can use your new domain name to address your serverless application.
## Creating and deploying Lambda functions
### Development Tips
- AWS Lambda does not support **async void** methods for its entry point (AKA function-handler). For making a Lambda function asynchronous you need to use **async Task** signature for your entry point.
- Using Syncronous calls like Task.Wait() or Task.Factory.RunSync() in a **synchronous lambda function** (the lambda function with sunc function handler) make it to run until it get a timeout exception. Use **async lambda function** instead.
- You cannot add a **SQS trigger** to a Lambda function when your queue is a FIFO (to find out the type of queue you can check it's name. FIFO queues have a .fifo postfix).
- Currently, there is no way for **batch insertion into a S3 bucket**. insert items in parallel for better performance.
