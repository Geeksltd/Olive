using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Olive
{
    /// <summary>
    /// When serializing objects it ignores all properties unless they have [Exposed] attribute.
    /// </summary>
    public class PessimisticJsonConverter : JsonConverter
    {
        static BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public;

        public override bool CanConvert(Type objectType) => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            Action<MemberInfo, object> add = (member, val) =>
            {
                writer.WritePropertyName(GetJsonName(member));
                serializer.Serialize(writer, val);
            };

            foreach (var property in value.GetType().GetProperties(Flags))
                if (property.Defines<JsonExposedAttribute>()) add(property, property.GetValue(value));

            foreach (var property in value.GetType().GetFields(Flags))
                if (property.Defines<JsonExposedAttribute>()) add(property, property.GetValue(value));

            writer.WriteEndObject();
        }

        static string GetJsonName(MemberInfo member)
        {
            var customName = member.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName;
            return customName.Or(member.Name);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = objectType.CreateInstance();

            foreach (var jsonProperty in JObject.Load(reader).Properties().ToList())
            {
                var objectProperty = objectType.GetProperties(Flags).FirstOrDefault(x => GetJsonName(x) == jsonProperty.Name);
                if (objectProperty != null)
                {
                    var castedValue = jsonProperty.Value.ToString().To(objectProperty.PropertyType);
                    objectProperty.SetValue(result, castedValue);
                }
                else
                {
                    var objectField = objectType.GetFields(Flags).FirstOrDefault(x => GetJsonName(x) == jsonProperty.Name);
                    if (objectField != null)
                    {
                        var castedValue = jsonProperty.Value.ToString().To(objectField.FieldType);
                        objectField?.SetValue(result, castedValue);
                    }
                }
            }

            return result;
        }
    }
}