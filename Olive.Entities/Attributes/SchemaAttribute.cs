using System;
using System.Reflection;

namespace Olive.Entities
{
    /// <summary>
    /// By default the table name of an entities is deemed to be named as the plural form of the type name.
    /// If it's specified as anything different, then this attribute will be added.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SchemaAttribute : Attribute
    {
        public string Schema { get; }

        public SchemaAttribute(string schema) => Schema = schema;

        public static string GetSchema(Type entityType) =>
            entityType.GetCustomAttribute<SchemaAttribute>(inherit: false)?.Schema;
    }
}