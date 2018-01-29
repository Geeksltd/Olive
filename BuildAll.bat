dotnet build Olive
dotnet build Olive.Entities
dotnet build Olive.Entities.Data
dotnet build Olive.Entities.Data.MySql
dotnet build Olive.Entities.Data.PostgreSQL
dotnet build Olive.Entities.Data.SQLite
dotnet build Olive.Entities.Data.SqlServer

dotnet build Services\Olive.Blob.Aws
dotnet build Services\Olive.Compression
dotnet build Services\Olive.Csv
dotnet build Services\Olive.Drawing
dotnet build Services\Olive.Excel
dotnet build Services\Olive.GeoLocation
dotnet build Services\Olive.PDF
dotnet build Services\Olive.PushNotification
dotnet build Services\Olive.SMS

dotnet build Integration\Olive.ApiClient
dotnet build Integration\Olive.ApiProxy
dotnet build Integration\Olive.Microservices

dotnet build Mvc\Olive.Web
dotnet build Mvc\Olive.Security
dotnet build Mvc\Olive.Mvc
dotnet build Mvc\Olive.Mvc.Email
dotnet build Mvc\Olive.Security.Impersonation
dotnet build Mvc\Olive.Mvc.Testing
dotnet build Mvc\Olive.Mvc.IpFilter
dotnet build Mvc\Olive.Mvc.NLog
dotnet build Mvc\Olive.Mvc.Security.Auth0
dotnet build Mvc\Olive.Mvc.Globalization
dotnet build Mvc\Olive.Hangfire