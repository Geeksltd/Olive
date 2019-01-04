using System;

namespace Olive.Entities
{
    /// <summary>
    /// When applied to a property, indicates that such property is AutoNumber (or Identity in SQL Server).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class BridgTableAttribute : Attribute
    {
        /// <summary>
        /// Determines if a given property is auto number.
        /// </summary>
        /// 
        public string TableName { get; set; }
        public BridgTableAttribute(string name)
        {
            TableName = name;
        }
    }
}