
# Olive compatibility change log

## 26 Oct 2020
Upgrade MSharp nuget to the latest version. Then change the following at the end of your `#Model.csproj`:
```xml
<Target Name="Generate code" AfterTargets="AfterBuild">
   <Exec Condition="'$(MSHARP_BUILD)' != 'FULL'" WorkingDirectory="$(TargetDir)" Command="dotnet msharp.dsl.dll /build /model" />
   <Exec Condition="'$(MSHARP_BUILD)' != 'FULL'" WorkingDirectory="$(TargetDir)" Command="start &quot;&quot; msharp /diagnose" />
</Target>
```

Do the same in #UI.csproj but change `/model` to `/ui`.

Previously, the code generation, and the design-time diagnostics (warnings, extension related files, etc) happened at the same time. This slowed down the build process. The above change will fix it so that:

- The `build` part (code generation) happens as part of the build, so that any issues still correctly break the build process.
- The diagnostics command is then executed in another process without slowing down your dev/build actions.

## 25 August 2020
We have moved `RegisterDataProvider()` method from `IDatabase` interface to `IDatabaseProviderConfig`. So if you have used something like this `Context.Current.Database().RegisterDataProvider(typeof(Service), new DataProvider());` please change it to this one:

```c#
var config = Context.Current.GetService<IDatabaseProviderConfig>();

config.RegisterDataProvider(typeof(Service), new DataProvider());
```


## 29 June 2020
- Replace `modifiedObjectType.IsCacheable()` with `Database.Cache().IsCacheable(modifiedObjectType)`

## 26 June 2020
We now support 3 database caching modes:
- `off` means disabled
- `single-server` is equivalent to the traditional enabled cache where everything is cached in memory statically, ideal for *vertical scaling*.
- `multi-server` is a new mode where data is cached in the scope of *each http request*. 

**Use the new mode instead of a disabled cache, for a significant performance boost in horizontally scaled applications.**

In `appSettings.json` change the cache setting from:
```json
  "Database": {
        ...
        "Cache": {
            "Enabled": true,
            ...
        },
```
to the following:
```json
  "Database": {
        ...
        "Cache": {
            "Mode": "single-server",
            ...
        },
```


## 28 May 2020
In the `SharedActionsController.cs` add IFileAccessorFactory the constructor like:
```csharp
    readonly IFileRequestService FileRequestService;
    readonly IFileAccessorFactory FileAccessorFactory;

    public SharedActionsController(
        IFileRequestService fileRequestService,
        IFileAccessorFactory fileAccessorFactory
    )
    {
        FileRequestService = fileRequestService;
	FileAccessorFactory = fileAccessorFactory;
    }
```
Also, apply the following change:
```csharp
	// Remove
	var accessor = await FileAccessor.Create(path, User);
	// Add
	var accessor = await FileAccessorFactory.Create(path, User);
```

## 12 May 2020
In the `Model` project => `Project.cs` change the following part.

From:
```csharp
AutoTask("Clean old temp uploads").Every(10, TimeUnit.Minute)
    .Run("await Olive.Mvc.FileUploadService.DeleteTempFiles(olderThan: 1.Hours());");
```

To:
```csharp
AutoTask("Clean old temp uploads").Every(10, TimeUnit.Minute)
    .Run(@"await Olive.Context.Current.GetService<Olive.Mvc.IFileRequestService>()
    .DeleteTempFiles(olderThan: 1.Hours());");
```
Also, in the `SharedActionsController.cs` add a constructor like:
```csharp
    readonly IFileRequestService FileRequestService;

    public SharedActionsController(IFileRequestService fileRequestService)
    {
        FileRequestService = fileRequestService;
    }
```
And change `UploadTempFileToServer` to:
```csharp
    [HttpPost, Authorize, Route("upload")]
    public async Task<IActionResult> UploadTempFileToServer(IFormFile[] files)
    {
        return Json(await FileRequestService.TempSaveUploadedFile(files[0]));
    }
```
Also change `DownloadTempFile` to:
```csharp
    [Route("temp-file/{key}")]
    public Task<ActionResult> DownloadTempFile(string key) => FileRequestService.Download(key);
```

## 23 Apr 2020
You can now specify the location where the temp uploaded files are stored. You need only one of the following settings.
```json
...
  "Blob": {
    "TempFileAbsolutePath": "",
    "TempFilePath": "",
  },
...
```

## 16 Apr 2020
A new property named `ItemGroup` is added to `IAuditEvent` so you need add the same to its implementer entity in your project.


## 31 Mar 2020
The `Entity` and `GlobalEntityEvents` classes have a number of lifecycle events such as `Saving`, `Saved`, etc.
Previously these were of the type `AsyncEvent`. That type is removed from Olive altogether due to the performance and memory management issues that it caused. Instead, those events are changed into normal .NET events. This means that where ever you handled such events, you should now:
- Use the `+=` operator rather than `.Handle()` method to attach handlers.
- The event handler method should return `void` rather than `Task`
- Your event handler argument should take `AwaitableEvent` or `AwaitableEvent<TArg>`
- If your event handling logic is sync, you simply write the code in the event handler method.

**Important:** If your event handler uses `await` you **should not** change the event handler to `async void`. Instead you should wrap your logic inside a call to the `Do(...)` method provided by the event handler method argument. For example:

```csharp
...
// Attach handler
GlobalEntityEvents.InstanceSaved.Handle(HandleInstanceSaved);
...

async Task HandleInstanceSaved(GlobalSaveEventArgs arg)
{
    ...
    await Somethind();
    ...
}
```

should be now written as:

```csharp
...
// Attach handler
GlobalEntityEvents.InstanceSaved += HandleInstanceSaved;
...
void HandleInstanceSaved(AwaitableEvent<GlobalSaveEventArgs> ev)
{
    ev.Do(async args => 
    {
       ...
       await Somethind();
       ...
    });
}
```




## 26 Nov 2019
You can remove `PredictableGuidEnabled` from the **appsettings.json**. As it is useless from now.
Also, Config should be remove from calling the `AddDevCommands`.

## 18 July 2019
If you are migrating from `Olive.MvcJs < v2` to `Olive.MvcJs > v2` please check 
[Javascript Fx (MvcJS) > Migration to version 2](https://geeksltd.github.io/Olive/#/MvcJS/MigrationAndDI) for the documentation.

## 19 March 2019
If your project is a Microservice, then:
- add the nuget package `Olive.Mvc.Microservices` to the `Website` project.
- In `Startup.cs` inherit from `Olive.Mvc.Microservices.Startup` and also remove `ConfigureSecurity(IApplicationBuilder app)`.
- Your `BaseController` class should inherit from `Olive.Mvc.Microservices.Controller`.


## 21 Feb 2019
Add the following mapping line to your `references.js` as shown below.
```javascript
...
map: {
       	"*": {
		....
           	"jquery-sortable": "jquery-ui/ui/widgets/sortable",
	}
}
...
```

## 23 Jan 2019
Change the following part in the Startup.cs as mentioned.= below.

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    base.ConfigureServices(services);
    .
    .
    .
    if (Environment.IsDevelopment())
	services.AddDevCommands(x => x.AddTempDatabase<SqlServerManager, ReferenceData>());
}
```


```csharp
public override void ConfigureServices(IServiceCollection services)
{
    base.ConfigureServices(services);
    .
    .
    .
    if (Environment.IsDevelopment())
	services.AddDevCommands(Configuration, x => x.AddTempDatabase<SqlServerManager, ReferenceData>());
}
```

Also, in order to benefit from `PredictableGuidGenerator` for the Pangolin batch runner add the following setting to the `appsettings.josn`.

```json
{
	"PredictableGuidEnabled": true
}
```

## 4 Jan 2019
M# will no longer generate the `DAL` files. They are handled at runtime now.

- Delete the `[Gen-DAL]` folder from  your `Domain` project.

- In **Domain.csproj**, remove the `[GEN-DAL]` folder form the project.

- In **Website.csproj** in the `appsetting.json` file remove the `Database:Providers` section:
```json
"Database": {
    "Providers": [
        {
            "AssemblyName": "Domain.dll",
            "ProviderFactoryType": "AppData.AdoDotNetDataProviderFactory"
        }
    ],
    ...
}
```
Should be changed to
```json
"Database": {    
    ...
}
```

- In the `startup.cs`, add the following line:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    base.ConfigureServices(services);
    services.AddDataAccess(x => x.SqlServer());
    ...
}
```

## 3 Jan 2019
Update `#Model.csproj` and `#UI.csproj` files add `/warn` to the end of the AfterBuild commands.

**#Model.csproj**
```xml
<Target Name="Generate code" AfterTargets="AfterBuild">
    <Exec Condition="'$(MSHARP_BUILD)' != 'FULL'" WorkingDirectory="$(TargetDir)" 
	  Command="dotnet msharp.dsl.dll /build /model /warn" />
</Target>
```
**#UI.csproj**
```xml
<Target Name="Generate code" AfterTargets="AfterBuild">
    <Exec Condition="'$(MSHARP_BUILD)' != 'FULL'" WorkingDirectory="$(TargetDir)" 
	  Command="dotnet msharp.dsl.dll /build /ui /warn" />
</Target>
```


## 7 Dec 2018
We have removed [Chosen](https://github.com/harvesthq/chosen) library and replaced it with [Bootstrap-select](https://github.com/snapappointments/bootstrap-select/), now you can safely remove `chosen` from your project.
1. Open `package.json` or `bower.json` and remove `chosen` then update `olive.mvc` to the version `0.9.175` and add `"bootstrap-select": "1.13.5",`
2. Open `references.js` and remove `chosen` and add the following lines:
```js
requirejs.config({
    baseUrl: '/lib',
    paths: {
        ....,
        "bootstrap-select": "bootstrap-select/dist/js/bootstrap-select"
    },
    ...
    shim: {
        ...
        "bootstrap-select": ['jquery', 'bootstrap'],
	...
        }
	...
});

requirejs([...., "bootstrap-select"
]);
...
```
3. Open `_common.scss` file and remove `chosen.css` and add the following lines:
```scss
@import "../../lib/bootstrap-select/sass/variables";
@import "../../lib/bootstrap-select/sass/bootstrap-select";
```

now your drop-down list will have default bootstrap style.

## 6 Dec 2018
- In `Website.csproj` set `<MvcRazorCompileOnPublish>true</MvcRazorCompileOnPublish>`
- In `Startup.cs` change use just `app.UseScheduledTasks<TaskManager>();` instead of the following block:
```csharp
if (Config.Get<bool>("Automated.Tasks:Enabled"))
    app.UseScheduledTasks(TaskManager.Run);
```

## 4 Dec 2018
In `Startup.cs` add an instance of `ILoggerFactory` to the constructor, and just pass it to the base constructor.
```csharp
public Startup(IWebHostEnvironment env, IConfiguration config, ILoggerFactory loggerFactory) 
       : base(env, config, loggerFactory)`
{
    ...
}
```

## 20 Nov 2018
We have used [FontAwesome 5](https://fontawesome.com/icons?d=gallery&s=brands,solid) in the last version of the M# and Olive so if your project is still using **FA4** please update it to the **FA5** as shown below:
1. Open `package.json` or `bower.json` and update **FontAwesome** to the last version.
2. Open `common.scss` and remove these lines:

`@import "../../lib/fontawesome/css/font-awesome.min.css";`

`$icon-font-path: '/fonts';` 

`$fa-font-path: $icon-font-path;`

3. Add these files:
```stylesheet
$fa-font-path: "/lib/@fortawesome/fontawesome-free/webfonts"; //override default fontawesome path.
@import "../../lib/@fortawesome/fontawesome-free/scss/fontawesome";
@import "../../lib/@fortawesome/fontawesome-free/scss/brands"; // free
@import "../../lib/@fortawesome/fontawesome-free/scss/solid"; // free
```
4. Run the `sass-to-css.bat` file and build your sass files.

5. We have updated Olive.MvcJs and project template to use custom radio and checkbox. If you are updating your bower package from any version before 0.9.161, you need to add some scss files to your project as you can find [here](https://github.com/Geeksltd/Olive.MvcTemplate/tree/master/Template/Website/wwwroot/styles/controls)

##### Checkbox's scss file:
```stylesheet
@import "../imports/variables";

$check-box-size: 20px;

input[type='checkbox'].handled {
    opacity: 0;

    &:focus + .checkbox-helper {
        box-shadow: 0 0 0 0 #fff, 0 0 0 0.2rem rgba(0,123,255,.25);
        border: 1px solid $input-border-focus;
    }

    &:checked + .checkbox-helper {

        &:before {
            outline: none;
            content: '\2714';
            font-size: 26px;
            position: absolute;
            top: -11px;
            left: 17px;
            color: $primary;
            font-family: initial;
        }
    }
}

.checkbox-helper {
    display: inline-block;
    width: $check-box-size;
    height: $check-box-size;
    border: 1px solid $input-border;
    border-radius: $input-border-radius;
    background-color: $input-bg;
}
```

##### Radio's scss file:
```stylesheet
@import "../imports/variables";

$check-box-size: 20px;
$check-box-size-checked-offset: 2px;

input[type='radio'].handled {
    opacity: 0;

    &:focus + .radio-helper {
        box-shadow: 0 0 0 0 #fff, 0 0 0 0.2rem rgba(0,123,255,.25);
        border: 1px solid $input-border-focus;
    }

    &:checked + .radio-helper {

        &:before {
            outline: none;
            display: inline-block;
            height: calc(#{$check-box-size} - #{$check-box-size-checked-offset});
            width: calc(#{$check-box-size} - #{$check-box-size-checked-offset});
            border-radius: 50%;
            content: '';
            position: absolute;
            top: -11px;
            left: 17px;
            background-color: $primary;
        }
    }
}

.radio-helper {
    display: inline-block;
    width: $check-box-size;
    height: $check-box-size;
    border: 1px solid $input-border;
    border-radius: 50%;
    background-color: $input-bg;
}
```

## 18 Nov 2018
- Rename FA5 to just FA

## 05 Nov 2018
We have added [bootstrap-select](https://developer.snapappointments.com/bootstrap-select/) to the OliveJs that has an effect on the **Collapsible** items. You can use this feature by updating `olive.mvc` to the version `0.9.154` or above and then do as below:
1. Open `package.json` or `bower.json` and add `"bootstrap-select" : "1.13.3"` library. *(you may need to run `yarn` to download the package again)*
2. Open `references.js` and change it as show below:
```javascript
requirejs.config({
		[...]
		paths: {
			[...]
			"bootstrap-select": "bootstrap-select/dist/js/bootstrap-select"
		},
		shim: {
			[...]
			"bootstrap-select": ['jquery', 'bootstrap'],
		}
	});
requirejs([ ... , "bootstrap-select"])]);
```
3. Open `common.scss` and import `bootstrap-select` and `variables` as show below:
```css
@import "../../lib/bootstrap-select/sass/bootstrap-select";
@import "../../lib/bootstrap-select/sass/variables";
```

run `sass-to-css.bat` too make sure everything is OK and there should be new UI where ever you have used `.AsCollapsibleCheckBoxList()` or `.Control(ControlType.CollapsibleCheckBoxList)`

## 30 Oct 2018
In `Website.csproj` file add the following to the first *PropertyGroup*:

```xml
<TieredCompilation>true</TieredCompilation>
```


## 29 Oct 2018
- Edit `#UI.csproj` and add `<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>` under the first <PropertyGroup>


## 25 Oct 2018
- Remove `Initialize.bat`
- In `dockerfile`, drop the `Website/` part from `COPY ./Website/publish/ .`
- Add [build.bat](https://raw.githubusercontent.com/Geeksltd/Olive.MvcTemplate/master/Template/Build.bat) to the solution root
- Edit the `csproj` files in `#Model` and `#UI` projects.
   - In the `<Exec ...` command add: `Condition="'$(MSHARP_BUILD)' != 'FULL'"`

## 3 Oct 2018
- If your application is a microservice (hosted in Hub), then in `BaseController.cs` add the following line:

  `protected override bool IsMicrofrontEnd => true;`

## 28 Sep 2018
- Replace `Cache.CanCache(modifiedObjectType)` with `modifiedObjectType.IsCacheable()`
- Replace `Database.Cache` with `Context.Current.Cache()` or `Database.Cache()`

## 27 Sep 2018
If you want to customise CKEditor toolbar you should follow these steps:
- Open `bower.json` and update `olive.mvc` to the version **0.9.140** or above.
- Create a JS file named `ckeditor_config.js` under scripts folder of your project and write your customisation code like this:
```javascript
CKEDITOR.editorConfig = function (config) {
    config.toolbar_Compact =
        [
            { name: 'basicstyles', items: ['Bold', 'Italic'] },
            { name: 'paragraph', items: ['NumberedList', 'BulletedList'] },
            { name: 'links', items: ['Link', 'Unlink'] }
        ];
};
```
- Open your main TS file that is usually located under scripts folder and in the `run()` method or any where that suit more add this code:
```javascript
var url: string = Url.effectiveUrlProvider("", null);
HtmlEditor.editorConfigPath = `${url}scripts/ckeditor_config.js`;
```
these codes may vary depending on your development environment, you should just set `HtmlEditor.editorConfigPath` with absolute path.

## 24 Sep 2018
**FormAction.ts** file in the Olive.MvcJs file has a dependency to the jQuery-UI **focusable** module. please add this to the `references.js` as shown below:

- Open `references.js` file and in the **paths** section of the `requirejs.config({...})` add `"jquery-ui/ui/focusable": "jquery-ui/ui/focusable",` right after `"jquery-ui/ui/widget"` then in the `requirejs([])` part add `"jquery-ui/ui/focusable"` after `"jquery-ui/ui/widget"`.

## 24 Sep 2018
You can redirect users to your custom login page by overriding `Url.onAuthenticationFailed`, for example here we redirect users to **Login.aspx**:
```javascript
Url.onAuthenticationFailed = () => { window.location.href = "/login.aspx"; }
```
These changes can be implemented in the project TS file before each ajax call.

By default, users will be redirected to `/login` path.

## 23 Sep 2018
- Replace `@Html.WebTestWidget()` with `@Html.DevCommandsWidget()` in all *cshtml* files.

## 21 Sep 2018
- Make your `ReferenceData` class implement `IReferenceData`
   - Remove `static` from all of its methods.
   - Add the following to the top of the file:
   ```csharp
   IDatabase Database;
   public ReferenceData(IDatabase database) => Database = database;
   ```
   - Change all calls to database to use this field instead.

#### Startup.cs changes
- From the `Configure()` method remove the following:
```csharp
if (Environment.IsDevelopment()) app.UseWebTest(config => config.AddTasks());
```
- From `OnStartUpAsync()` remove the following:
```csharp
 if (Environment.IsDevelopment())
     await app.InitializeTempDatabase<SqlServerManager>(() => ReferenceData.Create());
```
- In `ConfigureServices()` add the following line to the end of the method:
```csharp
if (Environment.IsDevelopment())
    services.AddDevCommands(x => x.AddTempDatabase<SqlServerManager, ReferenceData>()
    /* also if you use ApiClient: */ .AddClearApiCache()
    );
```


## 20 Sep 2018
- If you use the `Olive.Email` component, then:
  - In `Startup.cs` file, in the `ConfigureServices()` method, add `services.AddEmail();`
  - In `#Model\Project.cs`, change the AutoTask of email's code to:
  ```csharp
      AutoTask("Send emails")
           .Run(@"var outbox = Context.Current.GetService<Olive.Email.IEmailOutbox>();
            await outbox.SendAll();")
            ...;
  ```

## 19 Sep 2018
- In `references.js`, add urlArgs to the call to `requirejs.config()`. For example:
```javascript
requirejs.config({
    urlArgs: "v1", // Increment with every release to refresh browser cache.
    baseUrl: ...
```
Make sure that with every release of your application, you increment this number.
- In your `Views\Layouts\*.Container.cshtml` files, update the requirejs script's `data-main` value.
```html
<script src="~/lib/requirejs/require.js" data-main="/scripts/references.js?v=1"></script>
```

## 16 Sep 2018
If you're using AWS server identity (microservices with containers) please remove `services.AddAwsIdentity();` from `ConfigureServices(IServiceCollection services)` and add the following method:
```csharp
public Startup(IWebHostEnvironment env, IConfiguration config) : base(env, config)
{
    if (env.IsProduction()) config.LoadAwsIdentity();    
}
```

## 15 Sep 2018
* In `Startup.cs` class
   * From `ConfigureServices()` method, remove `AuthenticationBuilder.AddSocialAuth();`
   * Add the following method:
   ```csharp
    protected override void ConfigureAuthentication(AuthenticationBuilder auth)
    {
        base.ConfigureAuthentication(auth);
        auth.AddSocialAuth();
    }
    ```

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
public Startup(IWebHostEnvironment env, IConfiguration config) : base(env, config) { }
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
public override async Task OnStartUpAsync(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
        await app.InitializeTempDatabase<SqlServerManager>(() => ReferenceData.Create());
        
    // Add any other initialization logic that needs the database to be ready here.
}
```
- In `StartUp.cs` update the `Configure(...)` method to the following:
```csharp
public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
