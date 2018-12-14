using System.Reflection;

namespace Olive.Entities.ObjectDataProvider.V2
{
    public class PropertyData
    {
        internal static string DEFAULT_ID_COLUMN = "ID";

        public bool IsPrimaryKey { get; internal set; }

        public string Name { get; internal set; }

        public PropertyInfo PropertyInfo { get; internal set; }

        public bool IsAutoNumber { get; internal set; }

        public bool IsDeleted { get; internal set; }

        public bool IsOriginalId { get; internal set; }

        public bool IsUserDefined => (IsDeleted || IsOriginalId) == false;
    }
}