# Creating a new Microservice
1. In Visual Studio, click to create a new project.
2. Choose a short name to only specify the role of that service. Don't worry about the overall solution. For example if you're creating the **Calendar** microservice for the **Lorem** solution for **Ipsum** company, just name the new project *Calendar".
3. Select the template as **M# ASP.NET Core - MVC Microservice**
4. Choose *Sql Server* and click **Create**

## Website\appSettings.json
1. Open appSettings.json file
2. Set *"Microservices:Name"* to the short name of the microservice (e.g. just *"Calendar"*)
3. Set *"Microservices:Secret"* to a [new Guid value](https://www.guidgenerator.com/online-guid-generator.aspx).
4. Set *"Microservices:Root.Domain"* to the same value that you use for all the microservices you create for the same solution (e.g. *my-solution.dev.co*)
5. Set *"Authentication:CookieDataProtectorKey"* to the same secret value that you use in your Auth service.

## Website\Properties\LaunchSettings.json
1. Set **applicationUrl** to the correct value and with a unique port within your overall solution (e.g. calendar.my-solution.dev.co:9043)
