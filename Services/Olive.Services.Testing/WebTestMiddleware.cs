using Microsoft.AspNetCore.Http;
using Olive.Services.Excel;
using Olive.Web;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Services.Testing
{
    public class WebTestMiddleware
    {
        readonly RequestDelegate Next;

        public WebTestMiddleware(RequestDelegate next) => Next = next;

        public async Task Invoke(HttpContext context)
        {
            var terminateRequest = false;

            WebTestManager.AwaitReadiness();
            if (context?.Request?.Param("Web.Test.Command") == "Sql.Profile")
            {
                var file = await Entities.Data.DataAccessProfiler.GenerateReport(context.Request.Param("Mode") == "Snapshot").ToCsvFile();
                await context.Response.WriteAsync("Report generated: " + file.FullName);
                terminateRequest = true;
            }
            else
                await WebTestManager.ProcessCommand(context?.Request?.Param("Web.Test.Command"));

            if (!terminateRequest)
                await Next.Invoke(context);
        }
    }
}
