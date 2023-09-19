namespace Olive.Data.Replication.DataGenerator.UI.ModuleGenerators
{
    using Microsoft.Extensions.Primitives;
    using Olive.Entities;
    using Olive.Entities.Replication;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    public class ListModuleGenerator : BaseModuleGenerator, IModuleGenerator
    {
        public ListModuleGenerator(Type type, DestinationEndpoint endpoint) : base(type, endpoint) { }
        public string Action => "List";
        protected IDatabase Database => Context.Current.Database();
        public async Task<string> Render(string nameSpace, string typeName)
        {
            return base.Render(nameSpace, typeName, await BuildTable(nameSpace, typeName));
        }
        async Task<string> BuildTable(string nameSpace, string typeName) 
        {
            var props = await Database.Of(_type).Top(10).GetList();
            var columns = GetColumns();
            var r = new StringBuilder();
            r.AppendLine($"<a class=\"btn btn-primary\" role=\"button\" href = '/cmd/generate-{nameSpace}-{typeName}?action=new' >New</a>");
            r.AppendLine("<table class='table table-striped table-bordered'>");
            r.AppendLine("<tr>");
            GetColumns().Do(x => r.AppendLine($"<th scope='col'>{x.Name}</th>"));
            r.AppendLine("</tr>");
            foreach (var item in props)
                r.AppendLine(GetRow(columns, item));
            r.AppendLine("</table>");
            return r.ToString();
        }
        string GetRow(IEnumerable<PropertyInfo> columns, IEntity item)
        {
            var r = new StringBuilder();
            r.AppendLine("<tr>");
            foreach(var col in columns)
            {
                r.AppendLine($"<td>{col.GetValue(item)?.ToString() ?? string.Empty}</td>");
            }
            r.AppendLine("</tr>");
            return r.ToString();
        }
    }
}
