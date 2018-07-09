using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public class JsonNetResult : JsonResult
    {
        public JsonNetResult(object value) : base(value)
        {
            Settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Error,
                Formatting = Formatting.Indented
            };
        }

        public JsonNetResult(object value, JsonSerializerSettings serializerSettings) : base(value, serializerSettings)
        {
            Settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Error,
                Formatting = Formatting.Indented
            };
        }

        public JsonSerializerSettings Settings { get; }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var response = context.HttpContext.Response;
            response.ContentType = string.IsNullOrEmpty(ContentType) ? "application/json" : ContentType;

            if (Value == null) return;

            using (var writer = new StringWriter())
            {
                var scriptSerializer = JsonSerializer.Create(Settings);
                scriptSerializer.Serialize(writer, Value);

                var data = Encoding.UTF8.GetBytes(writer.ToString());

                await response.Body.WriteAsync(data, 0, data.Length);
            }

            return;
        }
    }
}
