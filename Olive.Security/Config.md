# Owin configurations

## Login with Google

1. Add Nuget package **Microsoft.Owin.Security.Google** to **Website** project.
1. Go to https://console.developers.google.com/
1. Create a project. Then click on that project.
1. Under *Dashboard* enable **Google+ API** and **Google+ Domains API** 
1. Under *Credentials* create a **OAuth Client ID**
   1.1. Click *Configure consent screen* and complete the page.
   1.2. Select **Web application** and give it a name
   1.3. Set *Authorised redirect URIs* to *http://YOURDOMAIN/signin-google*.
   1.4. Click **Create** and make a note of the generated *Client ID* and *Client secret*
1. Open *appsettings.json* in your website folder and set the following:
```javascript
"Authentication": {
   ...

    "Google": {
      "ClientId": "...",
      "ClientSecret": "...."
    }
 }
 ```
1. Add the following to your Uncomment the following.

        //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
        //{
        //    ClientId = Config.Get("Google.ClientId"),
        //    ClientSecret = Config.Get("Google.ClientSecret")
        //});
