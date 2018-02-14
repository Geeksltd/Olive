# Olive compatibility change log

## 14 Feb 2018
- Change *Context.Request* to **Context.Current.Request()**

## 24 Jan 2018
Change .Common.scss using the latest version from the template

## 19 Jan 2018
- Update bower.json to use bootstrap v4.0.0

## 18 Jan 2018
- Add Nuget reference to **Olive.Hangfire** in Website
- In FrontEnd.cshtml change @Html.ResetDatabaseLink() to **@Html.WebTestWidget()** 
- Make your StartUp.cs compatible with the new format:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    base.ConfigureServices(services);

    AuthenticationBuilder.AddSocialAuth();
    services.AddScheduledTasks();
}

public override void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseWebTest(ReferenceData.Create, config => config.AddTasks().AddEmail());
    base.Configure(app, env);

    if (Config.Get<bool>("Automated.Tasks:Enabled"))
        app.UseScheduledTasks(TaskManager.Run);
}
```
- From Startup.cs remove RegisterServiceImplementors() and CreateReferenceData() methods

## 17 Jan 2018
- In *#Model\Project.cs* remove all project settings except **Name(...)** and **SolutionFile(...)**.
- In Domain project delete Entities and DAL folders
- Delete App_Start\ViewLocationExpander.cs

# Up to 17 Jan 2018

### Update auth settings:
- Domain\...\AnonymousUser.cs
- Domain\Services\RoleStore.cs
- Domain\UserStore.cs

### Update javascript structure
* Delete the following from Website
  * ScriptReferences.json  
* Update the following files in **Website** from [the latest template](https://github.com/Geeksltd/Olive.MvcTemplate/blob/master/Template/Website/)
  * Views\Layouts\Common.Scripts.cshtml
  * bower.json (after changing, in VS right click it and select Restore Packages)
  * wwwroot\Scripts folder (everything unless you've created your own scripts)
  * tsconfig.json (if you don't have it already, but copy it in)

### Update SASS compilation
* Delete the following from Website
  * node_modules folder
  * Website\gulpfile.js
  * Website\package.json
* In Website.csproj, right before the </Project> node add:
```xml
 <UsingTask AssemblyFile="wwwroot\Styles\build\SassCompiler.exe" TaskName="WebCompiler.CompilerBuildTask" />
  <UsingTask AssemblyFile="wwwroot\Styles\build\SassCompiler.exe" TaskName="WebCompiler.CompilerCleanTask" />

  <Target Name="WebCompile" AfterTargets="PostBuildEvent" Condition="'$(RunWebCompiler)' != 'False'">
    <WebCompiler.CompilerBuildTask ContinueOnError="true" FileName="$(MSBuildProjectDirectory)\compilerconfig.json" />
  </Target>
  <Target Name="WebCompileClean" AfterTargets="CoreClean" Condition="'$(RunWebCompiler)' != 'False'">
    <WebCompiler.CompilerCleanTask ContinueOnError="true" FileName="$(MSBuildProjectDirectory)\compilerconfig.json" />
  </Target>
  ```
 * If you haven't changed the style files, replace the whole **wwwroot\Styles** folder from [the new temlate](https://github.com/Geeksltd/Olive.MvcTemplate/tree/master/Template/Website/wwwroot/Styles)
    * Otherwise just replace the folder  **wwwroot\Styles\build**
* Copy compilerconfig.json from the latest template (website root)
