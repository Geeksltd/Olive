using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Olive
{
    /// <summary>
    /// In order to use this class [DataContract] attribute must be used for class and [DataMember] attribute must be used for class fields.
    /// </summary>
    public class PessimisticJsonConverter //: JsonConverter
    {
        public string WriteJson(object value)
        {
            var stream = new MemoryStream();
            var serializer = new DataContractJsonSerializer(value.GetType());
            serializer.WriteObject(stream, value);
            byte[] json = stream.ToArray();
            stream.Close();
            return Encoding.UTF8.GetString(json, 0, json.Length);
        }

        public object ReadJson(string json, Type objectType)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var serializer = new DataContractJsonSerializer(objectType);
            var result = serializer.ReadObject(stream);
            stream.Close();
            return result;
        }

        //static BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public;

        //public override bool CanConvert(Type objectType) => true;

        //public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        //{
        //    writer.WriteStartObject();

        //    Action<MemberInfo, object> add = (member, val) =>
        //    {
        //        writer.WritePropertyName(GetJsonName(member));
        //        serializer.Serialize(writer, val);
        //    };

        //    foreach (var property in value.GetType().GetProperties(Flags))
        //        if (property.Defines<JsonExposedAttribute>()) add(property, property.GetValue(value));

        //    foreach (var property in value.GetType().GetFields(Flags))
        //        if (property.Defines<JsonExposedAttribute>()) add(property, property.GetValue(value));

        //    writer.WriteEndObject();
        //}

        //static string GetJsonName(MemberInfo member)
        //{
        //    var customName = member.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName;
        //    return customName.Or(member.Name);
        //}

        //public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        //{
        //    var result = objectType.CreateInstance();

        //    foreach (var jsonProperty in JObject.Load(reader).Properties().ToList())
        //    {
        //        var objectProperty = objectType.GetProperties(Flags).FirstOrDefault(x => GetJsonName(x) == jsonProperty.Name);
        //        if (objectProperty != null)
        //        {
        //            var castedValue = jsonProperty.Value.ToString().To(objectProperty.PropertyType);
        //            objectProperty.SetValue(result, castedValue);
        //        }
        //        else
        //        {
        //            var objectField = objectType.GetFields(Flags).FirstOrDefault(x => GetJsonName(x) == jsonProperty.Name);
        //            if (objectField != null)
        //            {
        //                var castedValue = jsonProperty.Value.ToString().To(objectField.FieldType);
        //                objectField?.SetValue(result, castedValue);
        //            }
        //        }
        //    }

        //    return result;
        //}
    }
}