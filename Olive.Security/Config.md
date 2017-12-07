# Owin configurations

## Login with Google

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
7. Open *Website\app_start\Startup.Auth.cs* and uncomment **EnableGoogle()**

## Login with Facebook
1. Go to https://developers.facebook.com/ and create a new app.
2. Inside your app, go to **Settings** and set **App Domains** and **Site URL** to the root of your website e.g. http://myproject.uat.co
3. From **Apps Dashboard**, copy *App ID* and *App Secret*
4. Open *appsettings.json* in your website folder and set the following:
```javascript
"Authentication": {
   ...
   "Facebook": {
      "AppID": "",
      "AppSecret": ""
    }
 }
 ```  
 5. Open *Website\app_start\Startup.Auth.cs* and uncomment **EnableFacebook()**

## Login with Microsoft
1. Go to https://apps.dev.microsoft.com, and create a new app.
2. Add a web platform and set *Redirect URLs* as *http://YOURDOMAIN/signin-microsoft*.
3. Generate a password and use in the next step.
4. Open *appsettings.json* in your website folder and set the following:
```javascript
"Authentication": {
   ...
   "Microsoft": {
      "ApplicationId": "",
      "Password": ""
    }
 }
 ```  
5. Open *Website\app_start\Startup.Auth.cs* and uncomment **EnableMicrosoft()**
