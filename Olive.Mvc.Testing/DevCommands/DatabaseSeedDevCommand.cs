using Microsoft.AspNetCore.Http;
using Olive.Entities.Data;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    class DatabaseSeedDevCommand : TempDatabaseDevCommand
    {
        readonly IHttpContextAccessor ContextAccessor;
        ITempDatabase TempDatabase;

        public DatabaseSeedDevCommand(IDatabaseServer databaseManager,
            ITempDatabase tempDatabase, IHttpContextAccessor contextAccessor)
          : base(databaseManager)
        {
            TempDatabase = tempDatabase;
            ContextAccessor = contextAccessor;
        }

        public override string Name => "db-seed";

        public override string Title => "Seed DB";

        public override async Task<string> Run()
        {
            var context = ContextAccessor.HttpContext;

            if (context.Request.Param("runner").HasValue())
                WebTestWidgetExtensions.IsUITestExecutionMode = true;

            await TempDatabase.Seed().ConfigureAwait(false);

            return null;
        }
    }
}