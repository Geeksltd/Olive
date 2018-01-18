using System;
using System.Text;
using Olive.Web;

namespace Olive.Services.Testing
{
    public class WebTestManager
    {
        public static string GetWebTestWidgetHtml()
        {
            var baseUrl = new Uri(Context.Request.ToAbsoluteUri()).RemoveQueryString("Web.Test.Command");

            var r = new StringBuilder();
            r.AppendLine(@"<div class='webtest-commands'
style='position: fixed; left: 49%; bottom: 0; margin-bottom: -96px; text-align: center; width: 130px; transition: margin-bottom 0.25s ease; background: #2ea8eb; color: #fff; font-size: 12px; font-family:Arial;'
onmouseover='this.style.marginBottom=""0""' onmouseout='this.style.marginBottom=""-96px""'>");

            r.AppendLine(@"<div style='width: 100%; background-color:#1b648d; padding: 3px 0;font-size: 13px; font-weight: 700;'>Test...</div>");

            foreach (var command in WebTestConfig.UserCommands)
            {
                var url = baseUrl.SetQueryString("Web.Test.Command", command.Key);
                r.AppendLine($@"<div style='width: 100%; padding: 4px 0;'><a href='{url}' style='color: #fff;'>{command.Value}</a></div>");
            }

            r.AppendLine("</div>");

            return r.ToString();
        }
    }
}