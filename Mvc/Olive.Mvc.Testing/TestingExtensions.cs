using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Olive.Mvc.Testing
{
    public static class TestingExtensions
    {
        public static IApplicationBuilder UseWebTest(this IApplicationBuilder app,
            Func<Task> createReferenceData,
            Action<IDevCommandsConfig> config = null)
        {
            if (!WebTestConfig.IsActive()) return app;

            WebTestConfig.ReferenceDataCreator = createReferenceData;

            var settings = new WebTestConfig();
            config?.Invoke(settings);

            if (settings.AddDefaultHandlers)
                settings.AddDatabaseManager()
                    .AddSnapshot()
                    .AddTimeInjector()
                    .AddSqlProfile()
                    .AddClearDatabaseCache();

            app.UseMiddleware<WebTestMiddleware>();

            Task.Factory.RunSync(() => TempDatabase.Create(enforceRestart: false, mustRenew: false));

            return app;
        }
    }
}

namespace Olive.Mvc
{
    public static class TestingExtensions
    {
        public static HtmlString WebTestWidget(this IHtmlHelper html)
        {
            if (!WebTestConfig.IsActive()) return null;

            if (Context.Current.Request().IsAjaxCall()) return null;

            if (WebTestConfig.IsAutoExecMode)
                html.RunJavascript("page.skipNewWindows();");

            return new HtmlString(WebTestManager.GetWebTestWidgetHtml());
        }
    }
}