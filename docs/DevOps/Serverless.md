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
## Creating and deploying Lambda functions
...
