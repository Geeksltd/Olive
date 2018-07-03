# Olive Javascripts
Every Olive solution adds a bower reference to the [**olive.mvc** package](https://github.com/Geeksltd/Olive.MvcJs/releases) which provides lightweight client-side framework for Olive applications. It consists of a number of useful elements:

- Extension methods for jQuery, String, Array and other low level types. E.g. string.trimStart(), string.contains(), etc.
- Helper utilities for general client-side coding related to url manipulation, form data, validation, modal (pop-up), etc.
- Plug-ins to convert basic html elements (e.g. input or select), into sophisticated ones (e.g. html editor, auto-complete, etc)
- A lightweight framework for a high performance SPA experience that is based on Razor templates and MVC.


## Typescript
Anders Hejlsberg, the genius father of Turbo Pascal, Delphi and C#, came up with yet another programming language that has dominated the web: Typescript. It's the scripting language of choice by Olive applications.

When you want to write custom javascript code in your application, you should create a typescript file (\*.ts) inside the *wwwroot\scripts* folder. The Typescript compiler will then create the \*.js version of your script in the same folder when you compile the website.

> wwwroot\scripts\components\myCustomModule.ts → wwwroot\scripts\components\myCustomModule.js


## requireJS
RequireJS is a JavaScript file and module loader. It is optimized for in-browser use, and can improve the speed and quality of your javascript code. In particular it provides the following benefits:
- Explicit dependencies between modules, listed in code.
- Asynchronous module loading.
- Lazy (on-demand) module loading.
- Support for one of the standard module wrappers called AMD, which is implemented by many open source libraries, making them easily available to your project.

requireJS is added by default to the Olive projects template.

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

### baseUrl
It sets the zero point for all script urls. Everything that proceeds in the rest of this file is measured in reference to this.
The value of */lib* means the *wwwroot/lib* folder in the website project source. You normally don't need to change this.

The url equivalent to that will be http://localhost:1234/lib. Unlike traditional ASP.NET, in ASP.NET Core the / url is mapped to the *wwwroot* folder as opposed to the website root folder (this is for security, so people can't download your source files).

### paths
In general, when using requireJS, you can load modules by their full path. But you can also load them by an alias which is a unique name that you define for each script file. The benfit of giving your javascript files an alias is that you will have a short reference to them for defining dependencies and desired loading order.

The 'paths' section in the requireJs config is where you define the aliases. The abovee example should be self-explanatory. The only point here is that the url part is measured in relation to the baseUrl. Also the .js extension will be automatically added.

For example:
- The url of a file at *Website\wwwroot\lib\some-lib\some-lib.js* will be "some-lib\some-lib".
- The url of a file at *Website\wwwroot\scripts\my-custom-file.js* will be "../scripts/my-custom-file".

### map
This setting allows you to define **address part aliaises** and map them to a url fragment. This is used for correcting incorrect addresses, to help them be resolved correctly.

Imagine that you are using a third party library and you don't want to change its source code. Inside that library it might be assuming a dependency to jquery.
- If that library had referenced jquery by its alias of 'jquery', all you had to do was to define that alias to map to the correct file in your project using the *paths* node as explained above.
- But imagine that the library had hard-coded the path as 'bower_components/jquery/jquery'. To solve this problem you can add an entry in the **map** config section to replace 'bower_components' to '.' so that the path would be evaluated as './jquery/jquery' (and . means the baseUrl).

The *map* config section is helpful when ever you see an error log in the Chrome console, complaining that a javascript file does not exist.

### shim
Often javascript files have a dependency on other scripts and hence they need to be loaded in the correct order. The traditional solution (Without using requireJs) was to list the `<script>` tags in the correct order in the html file. But requireJs works in a different way, and tried to parallelise script loading for improving performance when there is no known dependencies. 

What this means is that requireJs needs to know about those dependencies, or else it will load them in parallel, or in any random order, which can cause problems.

Script files can declare their dependencies by using the **import** statement, in which case requireJs would be able to understand that automatically.

Alternatively, and in particular for legacy libraries that don't do it, you need to declare the dependencies using the **shim** config section as demonstrated in the above example.

## Loading javascript files
The next statement after config, in **references.js**, is the load command:
```javascript
requirejs(["app/appPage", "olive/olivePage", "jquery", "jquery-ui", ...]);
```
Basically for each javascript file you want added, you add its alias to the array.

This means that to add a new Javascript file to the project (that you want loaded at the beginning and for every page) you can just:
1. Define it under **paths** in the config section.
2. Add its aliais to this array.

