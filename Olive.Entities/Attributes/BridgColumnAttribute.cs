using System;

namespace Olive.Entities
{
    /// <summary>
    /// When applied to a property, indicates that such property is AutoNumber (or Identity in SQL Server).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class BridgColumnAttribute : Attribute
    {
        /// <summary>
        /// Determines if a given property is auto number.
        /// </summary>
        ///     
        public string ColumnName { get; set; }
        public int Type { get; set; }
        public BridgColumnAttribute(string name, int type)
        {
            ColumnName = name;
            Type = type;
        }
    }
}