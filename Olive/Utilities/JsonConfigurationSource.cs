using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Olive
{
    public class JsonConfigurationSource : IConfigurationSource
    {
        readonly string Json;
        public JsonConfigurationSource(string json) => Json = json;

        public IConfigurationProvider Build(IConfigurationBuilder builder)
            => new JsonConfigurationProvider(Json);
    }

    public class JsonConfigurationProvider : ConfigurationProvider
    {
        private readonly string Json;
        private string CurrentPath;
        readonly Stack<string> Context = new Stack<string>();

        public JsonConfigurationProvider(string json) => Json = json;

        public override void Load()
        {
            Data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using var sr = new StringReader(Json);
            using var re = new JsonTextReader(sr) { DateParseHandling = DateParseHandling.None };
            VisitJObject(JObject.Load(re));
        }

        public IDictionary<string, string> GetData() => Data;

        void VisitJObject(JObject jObject)
        {
            foreach (var property in jObject.Properties())
            {
                EnterContext(property.Name);
                VisitToken(property.Value);
                ExitContext();
            }
        }

        void VisitToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    VisitJObject(token.Value<JObject>());
                    break;

                case JTokenType.Array:
                    VisitArray(token.Value<JArray>());
                    break;

                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Bytes:
                case JTokenType.Raw:
                case JTokenType.Null:
                    VisitPrimitive(token.Value<JValue>());
                    break;

                default:
                    throw new FormatException("Invalid Json format!!");
            }
        }

        void VisitArray(JArray array)
        {
            for (var index = 0; index < array.Count; index++)
            {
                EnterContext(index.ToString());
                VisitToken(array[index]);
                ExitContext();
            }
        }

        void VisitPrimitive(JValue data) => Data[CurrentPath] = data.ToString();

        void EnterContext(string context)
        {
            Context.Push(context);
            CurrentPath = ConfigurationPath.Combine(Context.Reverse());
        }

        void ExitContext()
        {
            Context.Pop();
            CurrentPath = ConfigurationPath.Combine(Context.Reverse());
        }
    }
}