using Microsoft.AspNetCore.Http;
using Olive.Entities.Data;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    class DatabaseCreateDevCommand : TempDatabaseDevCommand
    {
        readonly IHttpContextAccessor ContextAccessor;
        ITempDatabase TempDatabase;

        public DatabaseCreateDevCommand(IDatabaseServer databaseManager,
            ITempDatabase tempDatabase, IHttpContextAccessor contextAccessor)
          : base(databaseManager)
        {
            TempDatabase = tempDatabase;
            ContextAccessor = contextAccessor;
        }

        public override string Name => "db-create";

        public override string Title => "Create DB";

        public override async Task<string> Run()
        {
            var context = ContextAccessor.HttpContext;

            if (context.Request.Param("runner").HasValue())
                WebTestWidgetExtensions.IsUITestExecutionMode = true;

            await TempDatabase.ReCreateDb().ConfigureAwait(false);

            return null;
        }
    }
}