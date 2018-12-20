using System;
using System.Reflection;

namespace Olive.Entities.ObjectDataProvider.V2
{
    public class PropertyData
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

        public Action<IEntity, object> SetValue { get; }

        public Func<IEntity, object> GetValue { get; }

        public bool IsUserDefined => (IsDeleted || IsOriginalId || IsDefaultId) == false;

        public PropertyData(bool isBlob)
        {
            if (isBlob)
            {
                SetValue = SetBlobValue;
                GetValue = GetBlobValue;
            }
            else
            {
                SetValue = SetOtherValue;
                GetValue = GetOtherValue;
            }
        }

        void SetBlobValue(IEntity entity, object value) =>
            PropertyInfo.SetValue(entity, new Blob { FileName = value as string });

        void SetOtherValue(IEntity entity, object value) =>
            PropertyInfo.SetValue(entity, Convert.ChangeType(value, NonGenericType));

        object GetBlobValue(IEntity entity) => ((Blob)PropertyInfo.GetValue(entity))?.FileName;

        object GetOtherValue(IEntity entity) => PropertyInfo.GetValue(entity);
    }
}