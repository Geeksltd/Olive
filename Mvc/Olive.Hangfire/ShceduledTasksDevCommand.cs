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
            return @"<html>
                        <body>
                           <script src='https://ajax.googleapis.com/ajax/libs/jquery/2.1.3/ jquery.min.js'>
                           </script>
                           TODO: Add the hangfire tasks information here.
                        </body>
                     </html>";
        }
    }
}