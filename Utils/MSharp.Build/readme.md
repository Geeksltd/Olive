# M# Build Tool

To install this tool, in `cmd` run the following:

```
C:\> dotnet tool install --global msharp-build
```

## Create a new M# project
To create a new M# project, run the following command:
```
C:\Projects\> msharp-build /new /n:"Project.Name" [/t:https://github.com/Geeksltd/Olive.MvcTemplate/tree/master/Template] 
```
At this point, the template repository will be downloaded [from here](https://github.com/Geeksltd/Olive.MvcTemplate/tree/master/Template), and the placeholders will be replaced with the name provided in the `/n:` parameter. 

The `/t:...` parameter is optional, and defaults to the above. Alternatively, you can provide your own project template repository (which must be publically accessible).


## Build an existing project
To create a new M# project, run the following command:
```
C:\Projects\MyProject\> msharp-build
```

## Install build tools (prepare your environment)
You need to do this only once, to ensure your development environment is prepared, with all necessary tools installed:
```js
C:\>msharp-build /tools
```
