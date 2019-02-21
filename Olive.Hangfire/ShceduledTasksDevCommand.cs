using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Hangfire
{
    class ShceduledTasksDevCommand : IDevCommand
    {
        IHttpContextAccessor ContextAccessor;

        public ShceduledTasksDevCommand(IHttpContextAccessor accessor) => ContextAccessor = accessor;

        public string Name => "scheduled-tasks";

        public string Title => "Tasks...";

        public bool IsEnabled() => true;

        public async Task<string> Run()
        {
            var toRun = ContextAccessor.HttpContext.Request.Param("run");
            if (toRun.HasValue())
            {
                await BackgroundJobsPlan.Jobs[toRun].Action.Compile().Invoke();
            }

            return $@"<html>
                        <body>
                            <style>
                                *{{color: #545454;}}
                                tr:nth-child(even), thead{{background-color: #f2f2f2;}}
                                td, th{{padding: 3px 5px;}}
                            </style>
                            <script src='https://ajax.googleapis.com/ajax/libs/jquery/2.1.3/ jquery.min.js'>
                            </script>
                            <p>For more details check <a href=""{ContextAccessor.HttpContext.Request.GetAbsoluteUrl("/hangfire")}"">here</a>.</p>
                            <table>
                                <thead><tr>
                                    <th>Id</th>
                                    <th>Execute</th>                                    
                                </tr></thead>
                                {BackgroundJobsPlan.Jobs.Values.Select(ToMarkup).ToString("")}
                            </table>
                        </body>
                     </html>";
        }

        string ToMarkup(BackgroundJob job)
        {
            return $"<tr><td>{job.Name.HtmlEncode()}</td><td><a href='/cmd/scheduled-tasks?run={job.Name.ToPascalCaseId()}'>Execute</a></td></tr>";
        }
    }
}