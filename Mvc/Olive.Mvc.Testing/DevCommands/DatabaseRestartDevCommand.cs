using Microsoft.AspNetCore.Http;
using Olive.Entities.Data;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    class DatabaseRestartDevCommand : TempDatabaseDevCommand
    {
        readonly IHttpContextAccessor ContextAccessor;
        ITempDatabase TempDatabase;

        public DatabaseRestartDevCommand(IDatabaseServer databaseManager,
            ITempDatabase tempDatabase, IHttpContextAccessor contextAccessor)
          : base(databaseManager)
        {
            TempDatabase = tempDatabase;
            ContextAccessor = contextAccessor;
        }

        public override string Name => "db-restart";

        public override string Title => "Restart DB";

        public override async Task<bool> Run()
        {
            var returnUrl = ContextAccessor.HttpContext.Request.Headers["Referer"].ToString();

            var context = ContextAccessor.HttpContext;
            if (context.Request.Param("runner").HasValue())
                WebTestWidgetExtensions.IsUITestExecutionMode = true;

            await TempDatabase.Restart().ConfigureAwait(false);

            if (returnUrl.HasValue())
            {
                context.Response.Redirect(returnUrl);
                return true;
            }

            return false;
        }
    }
}
