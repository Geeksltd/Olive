using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Olive;
using System.Reflection;
using System.Xml;
using System.Text;
using Olive.Audit;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Olive
{
    public static class AuditExtensions
    {
        public static IServiceCollection AddDefaultAudit(this IServiceCollection @this)
        {
            @this.TryAddSingleton<Audit.IAudit, Audit.DefaultAudit>();

            return @this;
        }

        public static string RenderDataToHtml(this IAuditEvent applicationEvent, bool onlyNewChanges = false)
        {
            if (onlyNewChanges)
            {
                if (applicationEvent.Event.Equals("Update", caseSensitive: false))
                    return applicationEvent.ToHtml("//new");

                if (applicationEvent.Event.Equals("Insert", caseSensitive: false))
                    return applicationEvent.ToHtml("/*");
            }

            if (!applicationEvent.Event.Equals("Update", caseSensitive: false))
                return applicationEvent.ToHtml("//old");

            if (applicationEvent.Event.Equals("Delete", caseSensitive: false))
                return applicationEvent.ToHtml("//old");

            return string.Empty;
        }

        private static string ToHtml(this IAuditEvent applicationEvent, string parentNode)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(applicationEvent.ItemData);

            var ouputBuilder = new StringBuilder();
            ouputBuilder.AppendLine("<div style =\"padding: 10px;\">");

            var childNodes = xmlDoc.SelectSingleNode(parentNode);
            var linker = ":";
            if (parentNode == "//new")
                linker = "\u2192";

            foreach (XmlNode node in childNodes)
                ouputBuilder.AppendLine($"<span>{node.Name}</span> {linker} <span>{node.InnerText}</span><br>");

            ouputBuilder.AppendLine("</div>");
            return ouputBuilder.ToString();
        }

        public static async Task<string> RenderDataToStylishHtml(this IAuditEvent applicationEvent, bool onlyNewChanges = false)
        {
            if (onlyNewChanges)
            {
                if (applicationEvent.Event.Equals("Update", caseSensitive: false))
                    return await applicationEvent.ToStylishHtml("//new");

                if (applicationEvent.Event.Equals("Insert", caseSensitive: false))
                    return await applicationEvent.ToStylishHtml("/*");
            }

            if (applicationEvent.Event.Equals("Update", caseSensitive: false))
                return await applicationEvent.ToStylishHtml("//old", "background-color : #f0f0f0");

            if (applicationEvent.Event.Equals("Delete", caseSensitive: false))
                return await applicationEvent.ToStylishHtml("//old", "background-color : gray");

            return string.Empty;
        }

        private static async Task<string> ToStylishHtml(this IAuditEvent applicationEvent, string parentNode, string style = null)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(applicationEvent.ItemData);

            var item = await EntityProcessor.LoadItem(applicationEvent);
            var properties = item.GetType().GetProperties();

            var linkedProperties = item.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(pr => pr.PropertyType.IsClass && !pr.PropertyType.Assembly.FullName.StartsWith("Olive.Entities") && !pr.PropertyType.Assembly.FullName.StartsWith("System")).ToArray();


            var ouputBuilder = new StringBuilder();
            ouputBuilder.AppendLine($"<div style =\"padding: 10px;{style}\">");

            var childNodes = xmlDoc.SelectSingleNode(parentNode);
            string linker = ":";
            if (parentNode == "//new")
                linker = "\u2192";

            string color;

            foreach (XmlNode node in childNodes)
            {
                var prop = properties.FirstOrDefault(x => x.Name.Equals(node.Name, caseSensitive: false));

                if (linkedProperties.Any(x => prop.Name.StartsWith(x.Name, caseSensitive: false))) color = "#a22";
                else if (prop.GetPropertyOrFieldType() == typeof(Boolean)) color = "#22c";
                else color = "#333";

                ouputBuilder.AppendLine($"<span style=\"width:150px;text-align: right;color:#aaa\">{node.Name?.ToLiteralFromPascalCase()}</span> {linker} <span style=\"color:{color}\">{node.InnerText}</span><br>");
            }

            ouputBuilder.AppendLine("</div>");
            return ouputBuilder.ToString();
        }
    }
}
