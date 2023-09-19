namespace Olive.Data.Replication.DataGenerator.UI.ModuleGenerators
{
    using Olive.Entities;
    using Olive.Entities.Replication;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    internal class FormModuleGenerator : BaseModuleGenerator, IModuleGenerator
    {
        public string Action => "Form";
        public FormModuleGenerator(Type type, DestinationEndpoint endpoint) : base(type, endpoint) { }
        public Task<string> Render(string nameSpace, string typeName)
        {
            return Task.FromResult(base.Render(nameSpace, typeName, RenderForm(nameSpace, typeName)));
        }
        private string RenderForm(string nameSpace, string typeName)
        {
            var builder = new StringBuilder();
            var instance = _type.CreateInstance<IEntity>();
            builder.AppendLine($"<form method='post' action ='/cmd/generate-{nameSpace}-{typeName}'>");
            foreach (var column in GetColumns())
                builder.AppendLine(RenderField(column, instance));
            builder.AppendLine(" <button type=\"submit\" class=\"btn btn-primary\">Submit</button>");

            builder.AppendLine($"<a class=\"btn btn-secondry\" role=\"button\" href = '/cmd/generate-{nameSpace}-{typeName}' >Cancel</a>");
            builder.AppendLine("</form>");
            return builder.ToString();

        }
        private string RenderField(PropertyInfo prop, object item)
        {
            var builder = new StringBuilder();
            builder.AppendLine(" <div class=\"form-group row\" style={margin:20px}>");
            builder.AppendLine($"<label for='{prop.Name}' class='col-sm-2 col-form-label'>{prop.Name}</label>");
            builder.AppendLine("<div class=\"col-sm-10\">");
            builder.AppendLine($"<input type='{GetFieldType(prop)}' value='{GetDefaultValue(prop, item)}' name='{prop.Name}' class='{GetFieldClass(prop)}' placeholder='{prop.Name}'>");
            builder.AppendLine("</div>");
            builder.AppendLine("</div>");
            return builder.ToString();
        }
        private string GetFieldType(PropertyInfo prop)
        {
            if (prop.PropertyType == typeof(string) || prop.PropertyType == typeof(Guid))
                return "text";
            else if (prop.PropertyType == typeof(Boolean))
                return "checkbox";
            else return "text";
        }
        private string GetFieldClass(PropertyInfo prop)
        {
            if (prop.PropertyType == typeof(string) || prop.PropertyType == typeof(Guid))
                return "form-control";
            else if (prop.PropertyType == typeof(Boolean))
                return "form-check-input";
            else return "form-control";
        }
        private string GetDefaultValue(PropertyInfo prop, object item)
        {
            var val = prop.GetValue(item);
            if (prop.PropertyType == typeof(bool))
                return val?.ToString().ToLowerOrEmpty();
            return val?.ToString() ?? string.Empty;

        }
    }
}

