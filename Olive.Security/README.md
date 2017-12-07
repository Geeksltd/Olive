# Olive Security

Olive uses the standard built-in security model of ASP.NET and comes with 3 built-in mechanisms to authenticate the users:

### OAuth + OpenID (Social Media login)
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

### appsettings.json 
You will need to register your app with each 3rd party provider and configure your app with the secret tokens:
```json
"Authentication": {
   ...

    "Google": {
      "ClientId": "...",
      "ClientSecret": "...."
    }
 },
 ```

### Custom login page (via Auth0)
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

### appsettings.json 
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
