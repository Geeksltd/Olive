# Olive Security

Olive uses the standard built-in security model of ASP.NET and comes with 3 built-in mechanisms to authenticate the users:

## OAuth + OpenID (Social Media login)
In this model you effectively trust a third party such as Google, Facebook or Microsoft, for user authentication.
This is the preferred way to authenticate your users because you are relying on the giants to ensure that users are who they claim to be.

#### How to implement?
On your login page, add a button for each 3rd party provider.
For example a button reading "Login by Google" should invoke the following controller action:
```csharp
await OAuth.Instance.LoginBy("Google");
```
At this point, the user will be redirected to the Google login page and will be asked if they are happy to share their email with your website.
The user will then be redirected back to your website to the following URL:
>yourwebsite.com/ExternalLoginCallback

So you need a controller action to handle this and log the user in.
In the olive MVC project template there already is [a file to do this](https://github.com/Geeksltd/Olive.MvcTemplate/blob/master/Template/Website/Controllers/OAuthController.cs).

#### Registeration and config
You will need to register your app with each 3rd party provider and configure your app with the secret tokens.
[Learn how](Config.md)

## Custom login page (via Auth0)
If you don't want to redirect the users to a 3rd party website, you can capture the username and password directly in your website.
The user will then click your Login button where you should handle the authentication. 

You can of course create your own internal database of usernames and passwords. But it's not recommended as you would be then responsible for security, encryption, etc. Instead you can rely on a third party user store service such as [Auth0](https://auth0.com/) which is free for up to 7,000 active monthly users, which should be enough for most websites and business application. 

To use Auth0, your Login button handler action should do something like:
```csharp
[HttpPost, Route("LoginForm/Login")]
public async Task<ActionResult> Login(vm.LoginForm info)
{
    var authenticationResult = await Olive.Security.Auth0.Authenticate(info.Email, info.Password);
            
    if (!authenticationResult.Success)
    {
        Notify(authenticationResult.Message, "error");
        return await View(info);
    }
    else
    {
       var user = Domain.User.FindByEmail(info.Email);
       await user.LogOn();
       // TODO: Now redirect the user...
    }    
}
```

#### appsettings.json 
You will need to register your app with Auth0 and configure your app with the secret tokens:
```json
"Authentication": {
    ...

    "Auth0": {        
        "Domain": "samples.auth0.com",
        "ClientId": "....",
        "ClientSecret": "...."
    }
 },
 ```

## API callers (via JWT token)
The above two methods use browser cookies to carry the user identity in subsequent calls to your application. However there are cases where the client is not a browser, such as when calling WebAPI methods from another application.

In those cases the preferred method of authentication is via HTTP Header. One popular approach is through [JSON Web Token (aka JWT)](https://jwt.io/introduction/) which uses the standard HTTP HEADER authentication mechanism.

To make use of it, in your BaseController class, enable the \[JwtAuthenticate\] attribute.
```csharp
namespace Controllers
{
    [JwtAuthenticate]
    public class BaseController : Olive.Mvc.Controller
    {
        ...
    }
}
```
### API client sign-in
An API client, such as another service (in a Micro-services architecture) or a mobile app, should first log in to your login service in order to obtain a valid JWT token.

The log in service can be a controller action in the same application or a completely seperate application (e.g. for signle-sign-on). Once the token is obtained it should then be added to the HTTP Client header in every subsequent calls.

#### Mobile apps
In this scenario, the user will log on via the app using a username/password, or via a third party authentication provider.
Your logon web api function will generate a JWT token and send back to the client, which will then send it back (via HTTP HEADER) in all subsequent calls.

```csharp
public class UserController : BaseController
{
	[HttpPost, Route("users/login")]
	public object Login(LoginViewModel info)
	{
              // Check the username / password, etc and if successful, then:
	      return Olive.Security.JwtAuthentication.CreateTicket(user);
	}
}
````

#### Server identity
In this scenario, you provide a secret key to the client service through which it can be authenticated. When it then calls a web api it will be authenticated as its own identity, and not that of a user. This is usuaful when invokation of the API in your service is not related to any particular end user.

***Client service:***
In the client service application, you need the following code to enable you to create authenticated HttpClient instances which you can then use to invoke any actual Web API:
```csharp
static FileInfo CachedTokenFile => AppDomain.CurrentDomain.GetPath("App_Data\\Temp\\...txt").AsFile();

static async Task<string> GetToken()
{
    if (CachedTokenFile.Exists()) return CachedTokenFile.ReadAllText();
    
    using (var client = new HttpClient())
    {
        var result = await client.GetStringAsync(".../login?secret=" + Config.Get("ApiSecret"));
	await CachedTokenFile.WriteAllText(result);
	return result;
    }    
}

HttpClient CreateApiClient()
{
    var result = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
    return result;
}
````
Then to call an actual API function, simply use:
```csharp
...
using (var client = new CreateApiClient())
{
    await client.GetStringAsync(".../something");
    ...
}
```


#### Impersonated user identity
In this scenario, the user will have first logged on to the client service. Then as part of the process, the client service invokes an API function in your service ***on behalf of the user*** by just passing the user's authentication cookie.
To your service, it appears as if the user is directly sending a HTTP request.


## Single Sign On
For using SSO you need to provide a login service, which is basically a simple Olive MVC app.

Multiple apps should be set up as sub-domains of one main domain.

When authenticating, you should set the domain of the authentication cookie to the main domain. 

```csharp
protected override void ConfigureApplicationCookie(CookieAuthenticationOptions options)
{
     base.ConfigureApplicationCookie(options);
     options.Cookie.Domain = "mainDomain.com";
}
```
