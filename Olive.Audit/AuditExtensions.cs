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

        public static async Task<string> NewChangesToHtml(this IAuditEvent applicationEvent)
        {
            if (applicationEvent.Event == "Update")
                return await applicationEvent.ToHtml("//new");

            if (applicationEvent.Event == "Insert")
                return await applicationEvent.ToHtml("/*");

            return string.Empty;
        }

        public static async Task<string> OldChangesToHtml(this IAuditEvent applicationEvent)
        {
            if (applicationEvent.Event.IsAnyOf("Update", "Delete"))
                return await applicationEvent.ToHtml("//old", isOld: true);

            return string.Empty;
        }

        private static async Task<string> ToHtml(this IAuditEvent applicationEvent, string parentNode, bool isOld = false)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(applicationEvent.ItemData);

            var item = await EntityProcessor.LoadItem(applicationEvent);
            var properties = item.GetType().GetProperties();

            var linkedProperties = item.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(pr => pr.PropertyType.IsClass
                && !pr.PropertyType.Assembly.FullName.StartsWith("Olive.Entities")
                && !pr.PropertyType.Assembly.FullName.StartsWith("System")).ToArray();


            var ouputBuilder = new StringBuilder();

            if (isOld)
                ouputBuilder.AppendLine("<div class =\"audit-log-old\">");
            else
                ouputBuilder.AppendLine("<div class =\"audit-log-new\">");

            var childNodes = xmlDoc.SelectSingleNode(parentNode);
            var linker = parentNode == "//new" ? "\u2192" : ":";

            string propertyClass;

            foreach (XmlNode node in childNodes)
            {
                var prop = properties.FirstOrDefault(x => x.Name.Equals(node.Name, caseSensitive: false));

                if (linkedProperties.Any(x => prop.Name.StartsWith(x.Name, caseSensitive: false)))
                    propertyClass = "audit-log-property-linked";
                else if (prop.GetPropertyOrFieldType() == typeof(Boolean)) propertyClass = "audit-log-property-bool";
                else propertyClass = "audit-log-property";

                ouputBuilder.AppendLine($"<span class=\"audit-log-label\">{node.Name?.ToLiteralFromPascalCase()}</span> {linker} <span class=\"{propertyClass}\">{node.InnerText}</span><br>");
            }

            ouputBuilder.AppendLine("</div>");
            return ouputBuilder.ToString();
        }
    }
}
