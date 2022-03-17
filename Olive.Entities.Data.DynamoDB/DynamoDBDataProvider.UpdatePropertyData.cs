using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;

namespace Olive.Entities.Data
{
    public partial class DynamoDBDataProvider<T>
    {
        struct UpdatePropertyData
        {
            readonly static Type[] Ignores = new[]{
                typeof(JsonIgnoreAttribute),
                typeof(DynamoDBHashKeyAttribute),
                typeof(DynamoDBIgnoreAttribute),
            };

            public string Name { get; private set; }
            public object Value { get; private set; }
            bool IsString;
            bool IsBool;
            bool IsNumber;
            bool IsDateTime;
            bool CanRead;
            bool CanWrite;
            CustomAttributeData[] CustomAttributes;

            public static UpdatePropertyData FromProperty(PropertyInfo prop, object owner)
            {
                var propertyType = prop.PropertyType;
                if (propertyType.IsNullable())
                    propertyType = propertyType.GetGenericArguments()[0];

                return new UpdatePropertyData
                {
                    Name = prop.Name,
                    Value = prop.GetValue(owner),
                    IsString = propertyType == typeof(string),
                    IsBool = propertyType == typeof(bool),
                    IsNumber = propertyType.IsBasicNumeric(),
                    IsDateTime = propertyType == typeof(DateTime),
                    CanRead = prop.CanRead,
                    CanWrite = prop.CanWrite,
                    CustomAttributes = prop.CustomAttributes.ToArray(),
                };
            }

            public bool IsHashKey => CustomAttributes.Any(x => x.AttributeType == typeof(DynamoDBHashKeyAttribute));

            public AttributeValueUpdate GetAttributeValueUpdate()
            {
                var action = GetAttributeAction();

                if (action == AttributeAction.DELETE)
                    return new AttributeValueUpdate { Action = action };

                return new AttributeValueUpdate(GetAttributeValue(), action);
            }

            AttributeAction GetAttributeAction()
            {
                if (CustomAttributes.Any(x => x.AttributeType.IsAnyOf(Ignores))) return AttributeAction.DELETE;
                if (!CanRead || !CanWrite) return AttributeAction.DELETE;
                if (Value is null) return AttributeAction.DELETE;
                return AttributeAction.PUT;
            }

            public AttributeValue GetAttributeValue()
            {
                var newValue = new AttributeValue();

                if (IsString) newValue.S = Value.ToStringOrEmpty();
                else if (IsBool) newValue.BOOL = (bool)Value;
                else if (IsNumber) newValue.N = Value.ToString();
                else if (IsDateTime) newValue.S = ((DateTime)Value).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
                else newValue.S = Value.ToStringOrEmpty();

                return newValue;
            }
        }
    }
}