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

#### map
This setting allows you to define **address part aliaises** and map them to a url fragment. This is used for correcting incorrect addresses, to help them be resolved correctly.

Imagine that you are using a third party library and you don't want to change its source code. Inside that library it might be assuming a dependency to jquery.
- If that library had referenced jquery by its alias of 'jquery', all you had to do was to define that alias to map to the correct file in your project using the *paths* node as explained above.
- But imagine that the library had hard-coded the path as 'bower_components/jquery/jquery'. To solve this problem you can add an entry in the **map** config section to replace 'bower_components' to '.' so that the path would be evaluated as './jquery/jquery' (and . means the baseUrl).

The *map* config section is helpful when ever you see an error log in the Chrome console, complaining that a javascript file does not exist.

#### shim
