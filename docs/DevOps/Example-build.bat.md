# M# Build.bat example

Comments have been added to explain each step. 

### Note:
- The context in which this batch file is executed is the `Jenkins workspace` for the project, which looks similar to your local project directory. 
- All paths are relative to the project workspace unless an absolute path is specified.

```batch

@echo off

/// The first step to build the application is to build the Model project. For that the below script navigates to the M#\Model directory and runs "dotnet build" to build the project. Once built, the domain project will be generated. At any point if there is an error the whole execution will stop and the Jenkins build process will terminate.
ECHO ::::::::: Building #Model :::::::::::::::::::::::::::::::::::::::::::::
cd ..\..\M#\Model
call dotnet build -v q
call :cleanUp

if ERRORLEVEL 1 (goto error)

/// This step navigates to the Domain project and builds the project by running "dotnet build".
ECHO.
ECHO ::::::::: Building Domain :::::::::::::::::::::::::::::::::::::::::::::
cd ..\..\Domain
call dotnet build -v q
call :cleanUp
if ERRORLEVEL 1 (goto error)

/// TODO: confirm the reasoning with Paymon.
cd ..\Website
ECHO.
ECHO ::::::::: Installing YARN :::::::::::::::::::::::::::::::::::
call yarn install
if ERRORLEVEL 1 (goto error)

/// This line makes sure that all the bower components (if any) referenced by the application are installed. Obviously bower components are not pushed to the repository and have to be pulled when publishing the application. 
ECHO.
ECHO ::::::::: Installing Bower components :::::::::::::::::::::::::::::::::
if exist bower.json (
call bower install
if ERRORLEVEL 1 (goto error)
)

/// This line makes sure that all the TypeScript files (if any) referenced by the application are installed. We do not push any generated files to the repository so we have this line here to generate the js files for our TypeScripts.
ECHO.
ECHO ::::::::: Building typescript files :::::::::::::::::::::::::::::::::
call tsc
if ERRORLEVEL 1 (goto error)

// Same as TypeScript files and Bower components the generated css files are not pushed to the repository. Sass files are compiled using the script below.
ECHO.
ECHO ::::::::: Building sass files :::::::::::::::::::::::::::::::::
call wwwroot\Styles\build\SassCompiler.exe compilerconfig.json
if ERRORLEVEL 1 (goto error)

ECHO.
ECHO ::::::::: Restoring Olive DLLs ::::::::::::::::::::::::::::::::::::
call dotnet restore
if ERRORLEVEL 1 (goto error)

/// After compiling the Domain project we are ready to build UI. The script below navigates to the UI folder in M# directory and builds the code by running "dotnet build" which will generated the website for us.
ECHO.
ECHO ::::::::: Building #UI ::::::::::::::::::::::::::::::::::::::::::::::::
ECHO.
cd ..\M#\UI
call dotnet build -v q
if ERRORLEVEL 1 (goto error)

/// At this stage, all the js, css and bower components have been generated and downloaded. The Website project has been generated and everything is ready for the final step which is publishing the website. The script below publishes the website into the "publish" directory. 
ECHO.
ECHO ::::::::: Publishing the website ::::::::::::::::::::::::::::::::::::::::::::::::
ECHO.
cd ..\..\Website
call dotnet publish -o publish

exit /b 0

/// TODO: do we need this?
:cleanUp
echo ##################################
echo Cleaning up!
RMDIR "obj" /S /Q
echo ##################################
exit /b -1

:error
echo ##################################
echo Error occured!!!
echo Please run Initialize.bat again after fixing it.
echo ##################################
set /p cont= Press Enter to exit.
exit /b -1

```
