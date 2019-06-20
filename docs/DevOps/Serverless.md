# Serverless deployment on AWS Lambda
...

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
## Creating and deploying Lambda functions
...
