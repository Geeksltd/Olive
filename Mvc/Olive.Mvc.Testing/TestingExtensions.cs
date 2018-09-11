using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Olive.Entities.Data;
using System;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    public static class TestingExtensions
    {
        public static async Task InitializeTempDatabase<TDatabaseManager>(this IApplicationBuilder app, Func<Task> createReferenceData)
          where TDatabaseManager : DatabaseManager, new()
        {
            if (!WebTestConfig.IsActive()) return;

            TempDatabase.Config.DatabaseManager = new TDatabaseManager();
            WebTestConfig.ReferenceDataCreator = createReferenceData;
            await TempDatabase.AwaitReadiness();
        }

        public static IApplicationBuilder UseWebTest(this IApplicationBuilder app,
            Action<IDevCommandsConfig> config = null)
        {
            if (!WebTestConfig.IsActive()) return app;

            config?.Invoke(TempDatabase.Config);

            if (TempDatabase.Config.AddDefaultHandlers)
                TempDatabase.Config.AddDatabaseManager()
                    .AddTimeInjector()
                    .AddSqlProfile()
                    .AddClearDatabaseCache();

            app.UseMiddleware<WebTestMiddleware>();

            Entities.GuidEntity.NewIdGenerator = PredictableGuidGenerator.Generate;

            return app;
        }
    }
}

namespace Olive.Mvc
{
    using Olive.Mvc.Testing;

    public static class TestingExtensions
    {
        public static HtmlString WebTestWidget(this IHtmlHelper @this)
        {
            if (!WebTestConfig.IsActive()) return null;

            if (Context.Current.Request().IsAjaxCall()) return null;

            if (WebTestConfig.IsAutoExecMode)
                @this.RunJavascript("loadModule('olive/plugins/sanityAdapter', m => m.default.enable());");

            return new HtmlString(WebTestManager.GetWebTestWidgetHtml());
        }
    }
}