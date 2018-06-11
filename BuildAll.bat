@echo off

dotnet build Olive
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Entities
if ERRORLEVEL 1 (goto error)

dotnet build Olive.Audit
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

dotnet build Services\Olive.Blob.Aws
if ERRORLEVEL 1 (goto error)

dotnet build Services\Olive.Compression
if ERRORLEVEL 1 (goto error)

dotnet build Services\Olive.Csv
if ERRORLEVEL 1 (goto error)

dotnet build Services\Olive.Drawing
if ERRORLEVEL 1 (goto error)

dotnet build Services\Olive.Excel
if ERRORLEVEL 1 (goto error)

dotnet build Services\Olive.GeoLocation
if ERRORLEVEL 1 (goto error)

dotnet build Services\Olive.PDF
if ERRORLEVEL 1 (goto error)

dotnet build Services\Olive.PushNotification
if ERRORLEVEL 1 (goto error)

dotnet build Services\Olive.SMS
if ERRORLEVEL 1 (goto error)

dotnet build Integration\Olive.ApiClient
if ERRORLEVEL 1 (goto error)

dotnet build Integration\Olive.Microservices
if ERRORLEVEL 1 (goto error)

dotnet build Mvc\Olive.Web
if ERRORLEVEL 1 (goto error)

dotnet build Mvc\Olive.Security
if ERRORLEVEL 1 (goto error)

dotnet build Mvc\Olive.Mvc
if ERRORLEVEL 1 (goto error)

dotnet build Mvc\Olive.Mvc.Testing
if ERRORLEVEL 1 (goto error)

dotnet build Mvc\Olive.Email
if ERRORLEVEL 1 (goto error)

dotnet build Mvc\Olive.Security.Impersonation
if ERRORLEVEL 1 (goto error)

dotnet build Mvc\Olive.Mvc.IpFilter
if ERRORLEVEL 1 (goto error)

dotnet build Mvc\Olive.Mvc.Security.Auth0
if ERRORLEVEL 1 (goto error)

dotnet build Integration\Olive.ApiProxyGenerator
if ERRORLEVEL 1 (goto error)

dotnet build Mvc\Olive.Hangfire

exit /b 0

:error
set /p cont= Press Enter to exit.
exit /b -1