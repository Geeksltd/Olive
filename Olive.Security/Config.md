# Owin configurations

## Login with Google

1. Add Nuget package **Microsoft.Owin.Security.Google** to **Website** project.
2. Go to https://console.developers.google.com/
3. Create a project. Then click on that project.
4. Under *Dashboard* enable **Google+ API** and **Google+ Domains API** 
5. Under *Credentials* create a **OAuth Client ID**
   5.1. Click *Configure consent screen* and complete the page.
   5.2. Select **Web application** and give it a name
   5.3. Set *Authorised redirect URIs* to *http://YOURDOMAIN/signin-google*.
   5.4. Click **Create** and make a note of the generated *Client ID* and *Client secret*
6. Open *appsettings.json* in your website folder and set the following:
```javascript
"Authentication": {
   ...
    "Google": {
      "ClientId": "...",
      "ClientSecret": "...."
    }
 }
 ```
7. Open *Website\app_start\Startup.cs* and add the following to **Configure()** method:
```java
app.UseGoogleAuthentication(new GoogleOptions
{
    ClientId = Config.Get("Authentication.Google:ClientId"),
    ClientSecret = Config.Get("Authentication.Google:ClientSecret")
});
```
