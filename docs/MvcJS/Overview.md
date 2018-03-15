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
The value of */lib* means the *wwwroot/lib* folder in the website project source. You normally don't need to change this.

The url equivalent to that will be http://localhost:1234/lib. Unlike traditional ASP.NET, in ASP.NET Core the / url is mapped to the *wwwroot* folder as opposed to the website root folder (this is for security, so people can't download your source files).

#### paths
In general, when using requireJS, you can load modules by their full path. But you can also load them by an alias which is a unique name that you define for each script file. The benfit of giving your javascript files an alias is that you will have a short reference to them for defining dependencies and desired loading order.

The 'paths' section in the requireJs config is where you define the aliases. The abovee example should be self-explanatory. The only point here is that the url part is measured in relation to the baseUrl. Also the .js extension will be automatically added.

For example:
- The url of a file at *Website\wwwroot\lib\some-lib\some-lib.js* will be "some-lib\some-lib".
- The url of a file at *Website\wwwroot\scripts\my-custom-file.js* will be "../scripts/my-custom-file".
