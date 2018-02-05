using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Olive.Excel;
using Olive.Web;

namespace Olive.Mvc.Testing
{
    static class DefaultHandlers
    {
        static string Param(string key) => Context.Request.Param(key);

        static Task Respond(string response) => Context.Response.EndWith(response);

        internal static IWebTestConfig AddDatabaseManager(this IWebTestConfig config)
        {
            config.Add("dbChanges", () => DatabaseChangeWatcher.DispatchChanges());

            async Task<bool> startDatabase(bool shouldRedirect = false)
            {
                var redirect = Context.Request.ToAbsoluteUri().AsUri().RemoveQueryString("Web.Test.Command").ToString();

                WebTestConfig.SetRunner();
                await TempDatabase.Start();

                if (shouldRedirect)
                {
                    Debug.WriteLine("All done. Redirecting to: " + redirect);
                    Context.Response.Redirect(redirect);
                }

                return shouldRedirect;
            }

            foreach (var command in new[] { "start", "run", "ran", "cancel" })
                config.Add(command, () => startDatabase());

            config.Add("restart", () => startDatabase(true), "Restart DB");

            return config;
        }

        internal static IWebTestConfig AddSnapshot(this IWebTestConfig config)
        {
            bool shared() => Context.Request.Param("mode") == "shared";

            config.Add("snap", () =>
                  new Snapshot(Param("name"), shared()).Create(Context.Http));

            config.Add("restore", () =>
                 new Snapshot(Param("name"), shared()).Restore(Context.Http));

            config.Add("remove_snapshots", () => Snapshot.RemoveSnapshots(), "Kill DB Snapshots");

            config.Add("remove_snapshot", () => Snapshot.RemoveSnapshot(Param("name")));

            config.Add("snapshots_list", async () =>
            {
                await Respond(JsonConvert.SerializeObject(Snapshot.GetList(shared())));
                return true;
            });

            config.Add("snapExists", async () =>
            {
                await Respond(new Snapshot(Param("name"), shared()).Exists().ToString().ToLower());
                return true;
            });

            return config;
        }

        internal static IWebTestConfig AddSqlProfile(this IWebTestConfig config)
        {
            config.Add("Sql.Profile", async () =>
            {
                var file = await Entities.Data.DataAccessProfiler.GenerateReport(Param("Mode") == "Snapshot").ToCsvFile();
                await Respond("Report generated: " + file.FullName);
                return true;
            });

            return config;
        }

        internal static IWebTestConfig AddTimeInjector(this IWebTestConfig config)
        {
            config.Add("setLocalDate", async () =>
            {
                if (Param("date") == "now")
                {
                    // reset to normal
                    LocalTime.RedefineNow(overriddenNow: null);
                    await Respond(LocalTime.Now.ToString("yyyy-MM-dd @ HH:mm:ss"));
                }
                else
                {
                    var date = Param("date").TryParseAs<DateTime>() ?? LocalTime.Today;
                    var time = Param("time").TryParseAs<TimeSpan>() ?? LocalTime.Now.TimeOfDay;

                    date = date.Add(time);

                    var trueOrigin = DateTime.Now;

                    LocalTime.RedefineNow(() => date.Add(DateTime.Now.Subtract(trueOrigin)));
                    await Respond(date.ToString("yyyy-MM-dd @ HH:mm:ss"));
                }

                return true;
            });

            return config;
        }

        internal static IWebTestConfig AddServiceInjector(this IWebTestConfig config)
        {
            return config;
            // else if (command == "inject.service.response")
            // {
            //    var serviceType = AppDomain.CurrentDomain.GetAssemblies()
            //        .SelectMany(a => a.GetTypes())
            //        .Where(x => x.InhritsFrom(typeof(IntegrationService)))
            //        .SingleOrDefault(x => x.Name == request.Param("service"));

            //    if (serviceType == null)
            //        throw new Exception("Cannot find a class named " + request.Param("service") + " in the currently loaded assemblies, which inherits from IntegrationService<,>.");

            //    new Thread(new ThreadStart(async () =>
            //    await IntegrationTestInjector.Inject(serviceType, request.Param("request"), request.Param("response"))))
            //    { IsBackground = true }
            //   .Start();
            // } 
        }
    }
}