namespace Olive.Data.Replication.DataGenerator.UI.ModuleGenerators
{
    using Microsoft.AspNetCore.Http;
    using Olive.Entities;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Linq;
    using Olive.Entities.Replication;

    internal class FormSubmitGenerator : BaseModuleGenerator, IModuleGenerator
    {
        public string Action => "Form";
        public FormSubmitGenerator(Type type, DestinationEndpoint endpoint): base(type, endpoint) { }
        public async Task<string> Render(string nameSpace, string typeName)
        {

            try
            {
                var form = Request.Form;
                var item = (await Database.GetOrDefault(form["ID"].ToString(), _type))?.Clone();
                if (item == null) item = _type.CreateInstance<IEntity>();
                foreach (var column in GetColumns())
                {
                    var val = form.FirstOrDefault(x => x.Key == column.Name).Value.ToStringOrEmpty() ?? string.Empty;
                    column.SetValue(item, ConvertValue(column, val == "on" ? "true" : val));
                }
                try
                {
                    await SaveItem(item);
                }
                catch (ValidationException ex)
                {
                    return base.Render(nameSpace, typeName, RenderValidation(new List<string> { ex.Message }, nameSpace, typeName));

                }
            }
            catch(Exception ex)
            {
                return base.Render(nameSpace, typeName, RenderValidation(new List<string> { ex.Message }, nameSpace, typeName));
            }



            Response.Redirect($"/cmd/generate-{nameSpace}-{typeName}");
            return string.Empty;
        }
        private async Task SaveItem(IEntity entity)
        {
            await _endpoint.GetSubscriber(_type.Name).Import(entity.ToReplicationMessage(), SaveBehaviour.Default);
        }
        private string RenderValidation(List<string> messages, string nameSpace, string typeName)
        {
            var builder = new StringBuilder();
            builder.AppendLine("<h1> Validation Errors </h1>");
            builder.AppendLine("<ul>");
            foreach (var message in messages)
                builder.AppendLine($"<li>{message}</li>");
            builder.AppendLine("</ul>");

            builder.AppendLine($"<a class=\"btn btn-secondry\" role=\"button\" href = '/cmd/generate-{nameSpace}-{typeName}' >Cancel</a>");
            builder.AppendLine("</form>");
            return builder.ToString();

        }

        private object ConvertValue(PropertyInfo prop, string val)
        {
            return val.To(prop.PropertyType);
        }
    }
}

