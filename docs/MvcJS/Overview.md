# Olive Javascripts
...

## requireJS

## wwwroot\scripts\references.js
This is the primary file to configure requireJs and start-up libraries.
You will need to modify this file if you intend to add additional Javascript libraries to the project.

At the top of that file you will some something like:

```javascript
requirejs.config({
    baseUrl: '/lib',
    paths: {
        "jquery": "jquery/dist/jquery",
        "jquery-ui": "jqueryui/jquery-ui",
        ...       
    },
    map: {
        "*": {
            ...
            'olive': "olive.mvc/dist",
            "app": "../scripts"
        }
    },
    shim: {        
        "jquery-validate": ['jquery'],        
        "olive/olivePage": ["alertify", "olive/extensions/jQueryExtensions", "combodate"],
        ...
    }
});
```

This is a standard requireJs feature [and fully documented](http://requirejs.org/docs/api.html#config). Here is a brief summary of what you need to know:

#### baseUrl
It sets the zero point for all script urls. Everything that proceeds in the rest of this file is measured in reference to this.
The value of */lib* means the *wwwroot/lib* folder in the website project source. The url equivalent to that will be http://localhost:1234/lib. Unlike traditional ASP.NET, in ASP.NET Core the / url is mapped to the *wwwroot* folder as opposed to the website root folder (this is for security, so people can't download your source files).

#### paths
