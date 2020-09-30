

dotnet build Olive.Security.Aws
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Mvc
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Mvc.Paging
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Mvc.Testing
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Email
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Email.Imap
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Blob.Aws
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Blob.Azure
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Aws.Ses
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Security.Impersonation
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Mvc.IpFilter
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Mvc.Security.Auth0
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Hangfire
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Hangfire.MySql
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Security
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Mvc.Microservices
if ERRORLEVEL 1 (goto error)



exit /b 0

:error
set /p cont= Press Enter to exit.
exit /b -1