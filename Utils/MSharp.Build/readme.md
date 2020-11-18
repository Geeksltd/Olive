# M# Build Tool

To install this tool, in `cmd` run the following:

```
C:\> dotnet tool install --global msharp-build
```

## Create a new M# project
To create a new M# project, run the following command:
```
C:\Projects\> msharp-build /new /t:mvc /n:"Project Name" /...
```

## Build an existing project
To create a new M# project, run the following command:
```
C:\Projects\MyProject\> msharp-build
```

## Install build tools (prepare your environment)
You need to do this only once, to ensure your development environment is prepared, with all necessary tools installed:
```
C:\>msharp-build /tools
```
