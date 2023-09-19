namespace Olive.Data.Replication.DataGenerator.UI.ModuleGenerators
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Olive.Entities;
    using Olive.Entities.Replication;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    public abstract class BaseModuleGenerator
    {
        protected ILogger Log;
        protected DestinationEndpoint _endpoint;
        protected HttpRequest Request => Context.Current.Request();

        protected HttpResponse Response => Context.Current.Response();
        protected IDatabase Database => Context.Current.Database();
        protected Type _type { get; set; }
        public BaseModuleGenerator(Type type , DestinationEndpoint endpoint)

        {
            _endpoint = endpoint;
            _type = type;
            Log = Olive.Log.For(this);
        }
        public string Render(string nameSpace, string typeName, string Content)
        {
            var r = new StringBuilder();
            r.AppendLine("<!DOCTYPE html>");
            r.AppendLine("<html>");
            r.AppendLine("<head>");
            r.AppendLine("<link href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css\" rel=\"stylesheet\" integrity=\"sha384-1BmE4kWBq78iYhFldvKuhfTAU6auU8tT94WrHftjDbrCEXSU1oBoqyl2QvZ6jIW3\" crossorigin=\"anonymous\">");
            r.AppendLine("<style>");
            r.AppendLine("body {font-family:Arial; background:#fff; margin:20px; }");
            r.AppendLine("table { margin-bottom:20px; width:100% }");
            r.AppendLine("a { color: blue; display:block; float: right; margin-bottom:20px }");
            r.AppendLine("</style>");
            r.AppendLine("</head>");
            r.AppendLine("<body>");
            r.AppendLine($"<h2>Entity : {nameSpace}.{typeName}</h2>");
            r.AppendLine("<div class='container-fluid'>");
            r.AppendLine(Content);
            r.AppendLine("</div></body></html>");
            return r.ToString();
        }
        protected IEnumerable<PropertyInfo> GetColumns()
        {

            var props = _type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                .Where(e => e.CanWrite && e.CustomAttributes.None(w => w.AttributeType  == typeof(Newtonsoft.Json.JsonIgnoreAttribute)));

            var idProp = props.First(x => x.Name == "ID");
            var sorted = new List<PropertyInfo>() { idProp };
            sorted.AddRange(props.Except(idProp));
            return sorted;
        }
    }
}
