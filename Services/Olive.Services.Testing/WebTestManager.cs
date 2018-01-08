using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Olive.Entities;
using Olive.Services.Email;
using Olive.Services.Integration;
using Olive.Web;

namespace Olive.Services.Testing
{
    public class WebTestManager
    {
        internal static bool IsDatabaseBeingCreated;
        internal static bool? TempDatabaseInitiated;
        static bool? isTddExecutionMode;
        static Func<Task> ReferenceDataCreator;

        internal static void AwaitReadiness()
        {
            while (IsDatabaseBeingCreated) Thread.Sleep(100); // Wait until it's done.
        }

        public static string CurrentRunner { get; set; }

        /// <summary>
        /// Determines if the application is currently being ran by Sanity.
        /// </summary>
        public static bool IsSanityExecutionMode() => CurrentRunner == "Sanity";

        /// <summary>
        /// Registers a factory method that should be invoked upon creation of a new database to create reference objects.
        /// </summary>
        public static void CreateReferenceDataBy(Func<Task> referenceDataCreator)
            => ReferenceDataCreator = referenceDataCreator;

        /// <summary>
        /// Determines whether the application is running under Temp database mode.
        /// </summary>
        public static bool IsTddExecutionMode()
        {
            if (isTddExecutionMode.HasValue) return isTddExecutionMode.Value;

            var db = Config.GetConnectionString("AppDatabase").Get(c =>
                new SqlConnectionStringBuilder(c).InitialCatalog);

            db = db.Or("").ToLower().TrimStart("[").TrimEnd("]");

            isTddExecutionMode = db.EndsWith(".temp");

            return isTddExecutionMode.Value;
        }

        public static async Task InitiateTempDatabase(bool enforceRestart, bool mustRenew)
        {
            if (!IsTddExecutionMode()) return;

            IsDatabaseBeingCreated = true;
            var createdNew = false;

            try
            {
                SqlConnection.ClearAllPools();
                if (enforceRestart) TempDatabaseInitiated = null;
                if (TempDatabaseInitiated.HasValue) return;

                var generator = new TestDatabaseGenerator(isTempDatabaseOptional: true, mustRenew: mustRenew);
                TempDatabaseInitiated = generator.Process();
                createdNew = generator.CreatedNewDatabase;

                await Entity.Database.Refresh();
                SqlConnection.ClearAllPools();
            }
            finally { IsDatabaseBeingCreated = false; }

            if (ReferenceDataCreator != null && createdNew)
                // A new database is created. Add the reference data
                await ReferenceDataCreator();
        }

        /// <summary>
        /// It processes the command and returns true if the response is ready to be sent to the client.
        /// </summary>
        public static async Task<bool> ProcessCommand(string command)
        {
            if (command.IsEmpty()) return false;

            if (!IsTddExecutionMode()) throw new Exception("Invalid command in non TDD mode.");

            var request = Context.Http.Request;
            var response = Context.Http.Response;
            var result = false;

            var isShared = request.Param("mode") == "shared";

            if (command == "snap")
            {
                await new Snapshot(request.Param("name"), isShared).Create(Context.Http);
            }
            else if (command == "restore")
            {
                await new Snapshot(request.Param("name"), isShared).Restore(Context.Http);
            }
            else if (command == "remove_snapshots")
            {
                Snapshot.RemoveSnapshots();
            }
            else if (command == "snapshots_list")
            {
                await response.EndWith(JsonConvert.SerializeObject(Snapshot.GetList(isShared)));
                result = true;
            }
            else if (command == "snapExists")
            {
                if (new Snapshot(request.Param("name"), isShared).Exists())
                {
                    await response.EndWith("true");
                }
                else
                {
                    await response.EndWith("false");
                }

                result = true;
            }
            else if (command.IsAnyOf("start", "run", "ran", "cancel", "restart"))
            {
                await InitiateTempDatabase(enforceRestart: true, mustRenew: true);
                DatabaseChangeWatcher.Restart();
                if (request.Has("runner")) CurrentRunner = request.Param("runner");
            }
            else if (command == "testEmail")
            {
                await (await new EmailTestService(request, response).Initialize()).Process();
                result = true;
            }
            else if (command == "dbChanges")
            {
                DatabaseChangeWatcher.DispatchChanges();
            }
            else if (command == "tasks")
            {
                await DispatchTasksList();
            }
            else if (command == "setLocalDate")
            {
                if (request.Param("date") == "now")
                {
                    // reset to normal
                    LocalTime.RedefineNow(overriddenNow: null);
                    await response.EndWith(LocalTime.Now.ToString("yyyy-MM-dd @ HH:mm:ss"));
                }
                else
                {
                    var time = LocalTime.Now.TimeOfDay;
                    if (request.Has("time")) time = TimeSpan.Parse(request.Param("time"));

                    var date = LocalTime.Today;
                    if (request.Has("date")) date = request.Param("date").To<DateTime>();

                    date = date.Add(time);

                    var trueOrigin = DateTime.Now;

                    LocalTime.RedefineNow(() => { return date.Add(DateTime.Now.Subtract(trueOrigin)); });
                    await response.EndWith(date.ToString("yyyy-MM-dd @ HH:mm:ss"));
                }

                result = true;
            }
            else if (command == "remove_snapshot")
            {
                Snapshot.RemoveSnapshot(request.Param("name"));
            }
            else if (command == "inject.service.response")
            {
                var serviceType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(x => x.InhritsFrom(typeof(IntegrationService)))
                    .SingleOrDefault(x => x.Name == request.Param("service"));

                if (serviceType == null)
                    throw new Exception("Cannot find a class named " + request.Param("service") + " in the currently loaded assemblies, which inherits from IntegrationService<,>.");

                new Thread(new ThreadStart(async () =>
                await IntegrationTestInjector.Inject(serviceType, request.Param("request"), request.Param("response"))))
                { IsBackground = true }
               .Start();
            }

            return result;
        }

        /// <summary>
        /// To invoke this, send a request to /?web.test.command=tasks
        /// </summary>
        public static async Task DispatchTasksList()
        {
            throw new NotImplementedException("Hangfire?");

            // var response = Context.Http.Response;
            // var request = Context.Http.Request;

            // response.ContentType = "text/html";

            // response.WriteAsync("<html>").RunSynchronously();

            // response.WriteAsync("<body>").RunSynchronously();

            // response.WriteAsync("<script src='https://ajax.googleapis.com/ajax/libs/jquery/2.1.3/jquery.min.js'></script>").RunSynchronously();

            // response.WriteAsync("</body>").RunSynchronously();

            // response.WriteAsync("</html>").RunSynchronously();
        }

        public static string GetWebTestWidgetHtml(HttpRequest request)
        {
            var uri = new Uri(request.ToAbsoluteUri());
            var url = uri.RemoveQueryString("Web.Test.Command").ToString();
            if (url.Contains("?")) url += "&";
            else url += "?";

            return @"<div class='webtest-commands'
style='position: fixed; left: 49%; bottom: 0; margin-bottom: -96px; text-align: center; width: 130px; transition: margin-bottom 0.25s ease; background: #2ea8eb; color: #fff; font-size: 12px; font-family:Arial;'
onmouseover='this.style.marginBottom=""0""' onmouseout='this.style.marginBottom=""-96px""'
>
<div style='width: 100%; background-color:#1b648d; padding: 3px 0;font-size: 13px; font-weight: 700;'>Test...</div>
<div style='width: 100%; padding: 4px 0;'><a href='[URL]restart' style='color: #fff;'>Restart DB</a></div>
<div style='width: 100%; padding: 4px 0;'><a href='[URL]remove_snapshots' style='color: #fff;'>Kill DB Snapshots</a></div>
<div style='width: 100%; padding: 4px 0;'><a target='$modal' href='[URL]testEmail' style='color: #fff;'>Outbox...</a></div>
<div style='width: 100%; padding: 4px 0;'><a target='$modal' href='[URL]tasks' style='color: #fff;'>Tasks...</a></div>
</div>".Replace("[URL]", url + "Web.Test.Command=");
        }
    }
}