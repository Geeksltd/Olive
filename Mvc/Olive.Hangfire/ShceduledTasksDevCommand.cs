using System.Linq;
using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Http;
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
                                    <th>Last execution</th>
                                    <th>Last execution status</th>
                                    <th>Next execution</th>
                                </tr></thead>
                                {JobStorage.Current.GetConnection().GetRecurringJobs().Select(j => ToMarkup(j)).ToString("")}
                            </table>
                        </body>
                     </html>";
        }

        private string ToMarkup(RecurringJobDto job)
        {
            var dateFormat = "yyyy/MMM/dd H:mm:ss";
            string td(string value) => $"<td>{value}</td>";

            return $"<tr>{td(job.Id)}" +
                td(job.LastExecution.ToString(dateFormat)) +
                td(job.LastJobState) +
                $"{td(job.NextExecution.ToString(dateFormat))}</tr>";
        }
    }
}