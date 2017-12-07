# Owin configurations

## Login with Google


1. Add Nuget package *Microsoft.Owin.Security.Google* to *Website* project.
1. Go to https://console.developers.google.com/
   1. Create a project. Then click on that project.
   1. Under APIs&Auth > APIs enable "Google+ API" and "Google+ Domains API" 
   1. Under APIs&Auth > Credentials create a new Client ID and set Callback Address to http://YOURDOMAIN/signin-google
   1. Under APIs&Auth > Consent Screen you should configure the Product name, etc.
1. Add the following lines to your 
1. Add the following to your Uncomment the following.

        //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
        //{
        //    ClientId = Config.Get("Google.ClientId"),
        //    ClientSecret = Config.Get("Google.ClientSecret")
        //});
