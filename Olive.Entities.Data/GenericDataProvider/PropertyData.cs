using System;
using System.Reflection;

namespace Olive.Entities.Data
{
    public class PropertyData : IPropertyData
    {
        internal const string DEFAULT_ID_COLUMN = "ID";
        internal const string IS_MARKED_SOFT_DELETED = "IsMarkedSoftDeleted";
        internal const string ORIGINAL_ID = "OriginalId";

        public bool IsCustomPrimaryKey { get; internal set; }

        public string Name { get; internal set; }

        public string ParameterName { get; internal set; }

        public PropertyInfo PropertyInfo { get; internal set; }

        public Type NonGenericType { get; internal set; }

        public bool IsAutoNumber { get; internal set; }

        public bool IsDeleted { get; internal set; }

        public bool IsOriginalId { get; internal set; }

        public bool IsDefaultId { get; internal set; }

        public Type AssociateType { get; internal set; }

        public IPropertyAccessor Accessor { get; internal set; }

        internal string CustomDataConverterClassName { get; set; }

        public bool IsUserDefined => (IsDeleted || IsOriginalId || IsDefaultId) == false;
    }
}