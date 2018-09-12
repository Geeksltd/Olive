# Olive compatibility change log

## 12 Sep 2018
As it has been mentioned on the `21 August 2018` changes, we don't have a conflict between **jQuery-UI** and **Bootstrap** anymore so if you have removed `validation-style` according to the changes on the `04 July 2018` please reference it again. here are steps:

1. Open `bower.json` and add `"jquery-validation-bootstrap-tooltip": "*",` 
2. Open `references.js`
3. add `"validation-style": "jquery-validation-bootstrap-tooltip/jquery-validate.bootstrap-tooltip",` right after `"bootstrap": "bootstrap/dist/js/bootstrap",` in *paths* section
4. add `"validation-style": ['jquery', "jquery-validate", "bootstrap"],` in the `shim` section right after `"jquery-validate": ['jquery'],`
5. add `"validation-style"` after `"file-style"` in the `requirejs([...])`

## 12 Sep 2018
* Change the constructor in your Startup.cs file to the following.
   * In case you're using other parameters, you can include those too.
   * But as a minimum you should provide the following two arguments to the base constructor.   
```
public Startup(IHostingEnvironment env, IConfiguration config) : base(env, config) { }
```
* Remove the `env` parameter from all the other methods in that class. Use the inherited `Environment` field instead.

## 07 Sep 2018
There was an issue with [the chosen library](https://github.com/harvesthq/chosen/issues/515) and jQuery Validation. To fix this issue we make these changes in the `select.ts` file of the Olive.MvcJs project as shown below:
```javascript
selectControl.attr("style", "display:visible; position:absolute; clip:rect(0,0,0,0)");
```

## 05 Sep 2018
We have added an overload to `File()` method and now you can let pictures or files to be cached on the client side.
```csharp
return await File(accessor.Blob, cacheControl: new CacheControlHeaderValue { Public = true });
```
by setting cache header value to the public we let browsers to cache the file/picture.

## 04 Sep 2018
In Domain\ReferenceData.cs, change the implementation of the `Create<T>()` method to the following:
```csharp
static async Task<T> Create<T>(T item) where T : IEntity
{
    await Context.Current.Database().Save(item, SaveBehaviour.BypassAll);
    return item;
}
```
*Note: The ContinueWith() method used previously hides exceptions.*

## 21 August 2018
- The previous reference to the whole jQuery-UI has been removed and just Widget Component that was needed is referenced in RequiredJS. To update your current project please do as below:
1. Open `reference.js` and change `"jquery-ui": "jqueryui/jquery-ui"` to `"jquery-ui/ui/widget": "jquery-ui/ui/widget"` then remove `"jquery-ui/ui/widget": "jquery-ui"` from map section and change `"file-upload": ['jquery', 'jquery-ui']` to `"file-upload": ['jquery', 'jquery-ui/ui/widget']` and finally in requirejs method change `"jquery-ui"` to `"jquery-ui/ui/widget"` 
2. Open `bower.json`and change `"jqueryui": "1.12.1"` to `"jquery-ui": "^1.12.1"`

## 3 August 2018
- By default, all Modals are now based on Ajax call. if you want to used iFrame Modal please add `data-mode="iframe"` to you element and if you experience CSS issue with your form please add this line of code to the project CSS : `form { .group-control { flex: auto; } }`

## 15 July 2018
- In `Startup.cs` rename `ConfigureApplicationCookie` to `ConfigureAuthCookie`.

## 09 July 2018
- `EntityManager` class is retired. Instead, use `GlobalEntityEvents` or `Entity.Services.Xxx(...)`
- Remove the nuget package `Olive.Security`.
- Add the nuget packages `Olive.Mvc.Security` and `Olive.Encryption` instead.
- Fix GCop build performance by adding `<PreserveCompilationContext>true</PreserveCompilationContext>` and `<MvcRazorCompileOnPublish>false</MvcRazorCompileOnPublish>` to the Hub and Olive project templates.

## 04 July 2018
- Due to the conflict between jQuery-UI and Bootstrap, `jquery-validation-bootstrap-tooltip` package has been removed from `bower.json` and `references.js`
- Now Olive.MvcJs handles and shows UI validation errors using `ALERTIFY JS`

## 02 July 2018
- Install the latest Visual Studio update
- Install .NET Core 2.1 [SDK version 2.1.301](https://www.microsoft.com/net/download/thank-you/dotnet-sdk-2.1.301-windows-x64-installer) (or the latest)

## 28 June 2018
If you use *Email Sending* functionality in your application, to enable Sanity/Pangolin outbox feature, you should add `.AddEmail()` to the **WebTest Config**, resulting in the following:
```
...
app.UseWebTest(config => config.AddTasks().AddEmail());
...
```
When you restart the application, then, in the testing UI widget, at the bottom middle of the page, you will see a new menu item reading ```Outbox...```.

## 20 Jun 2018 - UPGRADE TO .NET CORE 2.1
- Install the latest .NET Core SDK from [here](https://www.microsoft.com/net/download/windows)
- In Initialize.bat, the second command should be changed to: 
```batch
echo {"runtimeOptions":{"tfm":"netcoreapp2.1","framework":{"name":"Microsoft.NETCore.App","version": "2.1.0"}}} >  M#\lib\netcoreapp2.1\MSharp.DSL.runtimeconfig.json
```
- Open the solution folder in CMD and run the above command also once manually.
- In your git ignore file, add:
```
M#/lib/*
!M#/lib/netcoreapp2.1/MSharp.DSL.runtimeconfig.json
```
- In Visual Studio, for all 4 projects in the solution, go to **Properties** and change the Target Framework to **Core 2.1**
- `Cookie.Expiration` is now obsolete. Use `ExpireTimeSpan` property in the `Startup.cs` instead.
- Update all nuget packages
- Remove the Nuget package **Microsoft.AspNetCore.All**.
- Instead install **Microsoft.AspNetCore.App** in the *Domain* project.
- Remove `.UseBrowserLink()` from `Startup.cs` class (if you have it)
- Clean and then build every project


## 02 May 2018
- In `StartUp.cs` add the following method:
```csharp
public override async Task OnStartUpAsync(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
        await app.InitializeTempDatabase<SqlServerManager>(() => ReferenceData.Create());
        
    // Add any other initialization logic that needs the database to be ready here.
}
```
- In `StartUp.cs` update the `Configure(...)` method to the following:
```csharp
public override void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    base.Configure(app, env);
    if (env.IsDevelopment()) app.UseWebTest(config => config.AddTasks());
    
    // The rest of your logic...
}
```


## 20 Apr 2018
- In references.js at the bottom of the file add the following:
```javascript
// Wait until Olive scripts are fully loaded before submitting any form
for (let i = 0; i < document.forms.length; i++) {
    document.forms[i].onsubmit = function (e) {
        if (window["IsOliveMvcLoaded"] === undefined) return false;
    };
}
```

## 17 Apr 2018
- Change all references to `Entity.Database` and `Database.Instance` to `Context.Current.Database()`
- In *appSettings.json* file Change `AppDatabase` to `Default`

## 13 Apr 2018
- In StartUp.cs, change `app.UseWebTest(...` to `app.UseWebTest<SqlServerManager>(...`.

## 11 Apr 2018
- In *appSettings.Development.json* change the `Logging` section to:
```json
  "Logging": {
    "IncludeScopes": true,
    "LogLevel": {
        "Default": "Debug",
        "System": "Information",
        "Microsoft": "Information",
        "Microsoft.AspNetCore.Authentication": "Warning",
        "Microsoft.AspNetCore.Authorization": "Warning",
        "Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor": "Warning",
        "Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker": "Warning"
    }
  }
```

## 9 Apr 2018
- The `Timeout` property of `ILoginInfo` has been changed from `TimeSpan` to `TimeSpan?`. Your user class should change accordingly to match the Api. 

## 8 Apr 2018
- In appSettings.json the `Blob` section should change to:
```json
    "Blob": {
        "RootPath": "Blob",
        "BaseUrl": "/file?",
        "WebTest": {
            "SuppressPersistence": false,
            "Origin": "..\\Test\\ReferenceFiles"
        }
    }
```

## 5 Apr 2018
- MergedForm<...?() method is changed. Instead of generic, send the type of the sub-form class. Alternatively, use a new overload of the MergeForm class which takes the settings of the sub-form as a lambda expression, so you don't have to define a class for the subform.

## 29 Mar 2018
- Add a NuGet reference to *Olive.Mvc.Paging* in the *Website* project
- The View() and some other helper methods on the BaseController now return an ActionResult instead of Task<ActionResult>. 
  
## 12 Mar 2018
-If you have any Export to excels in your application, add a NuGet reference to *Olive.Export* in the *Website* project

## 15 Feb 2018
- Change bower.json: *olive.mvc* to be *0.6.4* and **bootstrap-file-style** to be **2.1.0**
- Change the imports section of *bootstrap-social.scss* to [this](https://github.com/Geeksltd/Olive.MvcTemplate/blob/master/Template/Website/wwwroot/Styles/Imports/bootstrap-social.scss)
- Change **Common.scss** based on [this](https://github.com/Geeksltd/Olive.MvcTemplate/blob/master/Template/Website/wwwroot/Styles/Imports/Common.scss)
- Remove the import line from **nav.scss**
- Change **typeaheadjs.scss** based on [this](https://github.com/Geeksltd/Olive.MvcTemplate/blob/master/Template/Website/wwwroot/Styles/Imports/typeaheadjs.scss)
- Replace \_custom-variables.scss with [this](https://github.com/Geeksltd/Olive.MvcTemplate/blob/master/Template/Website/wwwroot/styles/Imports/_variables.scss)


## 14 Feb 2018
- Change *Context.Request* to **Context.Current.Request()**

## 5 Feb 2018
- IEmailQueueItem is now called IEmailMessage. A few of the fields have been changed:
  - EnableSsl -> no longer mandatory
  - Date -> Sendable date
  - Sender name -> From name
  - Sender address -> From address
  - Reply to name, Reply to address -> NEW

- IApplicationEvent is now called IAuditEvent and you need a reference to the package **Olive.Audit** in Domain:
  - Item key -> Item Id
  - IP -> User ip
  - Data -> Item data

- Update appsettings.json based on the template


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
