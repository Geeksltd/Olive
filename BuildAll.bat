@echo off

dotnet build Olive
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Entities
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Audit
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Encryption
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Entities.Data
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Entities.Data.MySql
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Entities.Data.PostgreSQL
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Entities.Data.SQLite
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Entities.Data.SqlServer
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Entities.Cache.Redis
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Aws
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Blob.Aws
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Compression
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Csv
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Drawing
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Excel
if ERRORLEVEL 1 (goto error)

dotnet build Olive.GeoLocation
if ERRORLEVEL 1 (goto error)

dotnet build Olive.PDF
if ERRORLEVEL 1 (goto error)

dotnet build Olive.PushNotification
if ERRORLEVEL 1 (goto error)

dotnet build Olive.SMS
if ERRORLEVEL 1 (goto error)

dotnet build Olive.ApiClient
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Microservices
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Web
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Security
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Mvc
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Mvc.Testing
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Email
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Security.Impersonation
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Mvc.IpFilter
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Mvc.Security.Auth0
if ERRORLEVEL 1 (goto error)

dotnet build Olive.ApiProxyGenerator
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Hangfire
if ERRORLEVEL 1 (goto error)

exit /b 0

:error
set /p cont= Press Enter to exit.
exit /b -1