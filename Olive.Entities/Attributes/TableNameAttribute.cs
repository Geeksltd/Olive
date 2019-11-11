using System;
using System.ComponentModel;
using System.Reflection;

namespace Olive.Entities
{
    /// <summary>
    /// By default the table name of an entities is deemed to be named as the plural form of the type name.
    /// If it's specified as anything different, then this attribute will be added.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TableNameAttribute : Attribute
    {
        public string TableName { get; }

        public TableNameAttribute(string tableName) => TableName = tableName;

        public static string GetTableName(Type entityType)
        {
            var result = entityType.GetCustomAttribute<TableNameAttribute>(inherit: false)?.TableName;
            if (result.HasValue()) return result;

            var title = entityType.GetCustomAttribute<DisplayNameAttribute>(inherit: false)?.DisplayName;
            title = title.Or(entityType.Name.ToLiteralFromPascalCase());

            return title.ToPlural().ToPascalCaseId();
        }
    }
}