# Example PrepairServer.bat file

- It first installs `[Chocolatey](https://chocolatey.org/)` which is a package manager for Windows. It also adds the Chocolatey executable path to the PATH variable so that the "choco" command is available in the build process.

- It installs `yarn` and `bower` using `Chocolatey`.
- It installs `webpack` using `yarn`
- It installs `TypeScript` using `npm`.

```batch
@echo off

ECHO ::::::::: Ensuring Chocolatey is installed ::::::::::::::::::::::
WHERE choco > nul
if ERRORLEVEL 1 (
	ECHO ::::::::: Installing Chocolatey  ::::::::::::::::::::::
	call powershell -NoProfile -InputFormat None -ExecutionPolicy Bypass -Command "iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))" 
	call SET "PATH=%PATH%;%ALLUSERSPROFILE%\chocolatey\bin;"
	call choco feature enable -n allowGlobalConfirmation
	ECHO ::::::::: Installed Chocolatey  ::::::::::::::::::::::
) 


ECHO ::::::::: Ensuring Yarn installed (globally) ::::::::::::::::::::::
WHERE yarn > nul
if ERRORLEVEL 1 (
call choco install yarn
)

WHERE yarn > nul
if ERRORLEVEL 1 (goto error)


ECHO ::::::::: Ensuring Typescript compiler installed (globally) ::::::::::::::::::::::
WHERE tsc > nul
if ERRORLEVEL 1 (
call npm install -g typescript
)

WHERE tsc > nul
if ERRORLEVEL 1 (goto error)


ECHO ::::::::: Ensuring WebPack is installed (globally) ::::::::::::::::::::::
call yarn global add webpack
if ERRORLEVEL 1 (goto error)

ECHO ::::::::: Ensuring Bower (globally) ::::::::::::::::::::::
WHERE bower > nul
if ERRORLEVEL 1 (
call choco install bower
)

WHERE bower > nul
if ERRORLEVEL 1 (goto error)

exit /b 0

:error
echo ##################################
echo Error occured!!!
echo Please run Initialize.bat again after fixing it.
echo ##################################
set /p cont= Press Enter to exit.
PAUSE
exit /b -1

```
