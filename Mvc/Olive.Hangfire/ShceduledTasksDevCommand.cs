using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Hangfire
{
    class ShceduledTasksDevCommand : IDevCommand
    {
        IHttpContextAccessor ContextAccessor;

        public ShceduledTasksDevCommand(IHttpContextAccessor contextAccessor)
        {
            ContextAccessor = contextAccessor;
        }

        public string Name => "scheduled-tasks";

        public string Title => "Tasks...";

        public bool IsEnabled() => true;

        public async Task<bool> Run()
        {
            await ContextAccessor.HttpContext.Response.EndWith(@"
         <html>
            <body>
               <script src='https://ajax.googleapis.com/ajax/libs/jquery/2.1.3/ jquery.min.js'>
               </script>
               TODO: Add the hangfire tasks information here.
            </body>
         </html>");

            return true;
        }
    }
}