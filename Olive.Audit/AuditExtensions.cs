using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Olive;
using System.Reflection;
using System.Text;
using Olive.Audit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Olive.Entities;
using System.Xml.Linq;

namespace Olive
{
    public static class AuditExtensions
    {
        public static IServiceCollection AddDefaultAudit(this IServiceCollection @this)
        {
            @this.TryAddTransient<IAudit, DefaultAudit>();
            return @this;
        }

        public static async Task<string> NewChangesToHtml(this IAuditEvent applicationEvent)
        {
            if (applicationEvent.Event == "Update")
                return await applicationEvent.ToHtml("new");

            if (applicationEvent.Event == "Insert")
                return await applicationEvent.ToHtml();

            return string.Empty;
        }

        public static async Task<string> OldChangesToHtml(this IAuditEvent applicationEvent)
        {
            if (applicationEvent.Event.IsAnyOf("Update", "Delete"))
                return await applicationEvent.ToHtml("old");

            return string.Empty;
        }

        static async Task<string> ToHtml(this IAuditEvent applicationEvent, string parentNode = null)
        {
            try
            {
                var item = await EntityProcessor.LoadItem(applicationEvent, parentNode == "new");

                if (item == null)
                    throw new("Could not load the type " + applicationEvent.ItemType);

                var properties = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var ouputBuilder = new StringBuilder();

                if (parentNode == "old")
                    ouputBuilder.AppendLine("<div class =\"audit-log audit-log-old\">");
                else
                    ouputBuilder.AppendLine("<div class =\"audit-log audit-log-new\">");

                var elements = XElement.Parse(applicationEvent.ItemData).Elements();
                elements = parentNode.HasValue() ? elements.FirstOrDefault(x => x.Name.LocalName == parentNode)?.Elements() : elements;

                var linker = parentNode == "new" ? "\u2192" : ":";

                foreach (var element in elements.ToArray())
                {
                    var propertyClass = "audit-log-property";
                    var property = properties.FirstOrDefault(x => x.Name == element.Name.LocalName);

                    if (property == null) continue;

                    if (property.IsEntity(item))
                        propertyClass = $"{propertyClass} audit-log-property-entity";
                    else if (property.PropertyType == typeof(bool))
                        propertyClass = $"{propertyClass} audit-log-property-bool";

                    ouputBuilder.AppendLine($"<span class=\"audit-log-label\">{element.Name.LocalName?.ToLiteralFromPascalCase()}</span> {linker} <span class=\"{propertyClass}\">{element.Value}</span><br>");
                }

                ouputBuilder.AppendLine("</div>");
                return ouputBuilder.ToString();
            }
            catch (Exception ex)
            {
                return "⚠ Failed to load: " + ex.Message;
            }
        }

        public static bool IsEntity(this PropertyInfo property, object obj)
        {
            if (obj == null || property == null) return false;

            var type = property.PropertyType;

            return (type.InhritsFrom(typeof(IEntity))) || (type.IsClass
                && !type.Assembly.FullName.StartsWith("Olive.Entities")
                && !type.Assembly.FullName.StartsWith("System"));
        }
    }
}