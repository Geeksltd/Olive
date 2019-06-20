# Serverless deployment on AWS Lambda
...

## Deploying M# ASP.NET web app as a Serverless Application
### Understanding the roles
There are two security principal concepts here:
1. **Runtime role:** This is a role used at runtime by the lambda function. The application essentially runs under this role. It needs to have as little permissions as possible. For example if the function needs to download data from a certain S3 bucket, it should have access to that, and nothing more.
## Creating and deploying Lambda functions
...
