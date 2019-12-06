@echo off

ECHO.
ECHO ::::::::: Rebuilding sass files :::::::::::::::::::::::::::::::::
ECHO.
call SassCompiler.exe ..\..\..\Compilerconfig.json
