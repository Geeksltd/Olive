using System;
using System.Collections.Generic;
using System.Linq;

namespace Olive.Entities.Data
{
    public class DataProviderMetaData : IDataProviderMetaData
    {
        IDataProviderMetaData[] baseClassesInOrder, drivedClassesInOrder;
        string idColumnName;
        bool? hasAutoNumber;
        IPropertyData autoNumberProperty;

        public string TableName { get; internal set; }

        public string TableAlias { get; internal set; }

        public string Schema { get; internal set; }

        public IPropertyData[] Properties { get; internal set; }

        public Type[] BaseClassTypesInOrder { get; internal set; }

        public Type[] DrivedClassTypes { get; internal set; }

        public Type Type { get; }

        public IDataProviderMetaData[] BaseClassesInOrder
        {
            get
            {
                if (baseClassesInOrder == null)
                    baseClassesInOrder = BaseClassTypesInOrder.Select(b => DataProviderMetaDataGenerator.Generate(b)).ToArray();

                return baseClassesInOrder;
            }
        }

        public IDataProviderMetaData[] DrivedClasses
        {
            get
            {
                if (drivedClassesInOrder == null)
                    drivedClassesInOrder = DrivedClassTypes.Select(b => DataProviderMetaDataGenerator.Generate(b)).ToArray();

                return drivedClassesInOrder;
            }
        }

        public IEnumerable<IPropertyData> UserDefienedAndIdAndDeletedProperties => 
            Properties?.Where(p => p.IsUserDefined || p.IsDefaultId || p.IsDeleted);

        public IEnumerable<IPropertyData> UserDefienedAndDeletedProperties => Properties?.Where(p => p.IsUserDefined || p.IsDeleted);

        public IEnumerable<IPropertyData> UserDefienedProperties => Properties?.Where(p => p.IsUserDefined);

        public IEnumerable<IPropertyData> AssociateProperties => Properties?.Where(p => p.AssociateType != null);

        public string IdColumnName
        {
            get
            {
                if (idColumnName.IsEmpty())
                    idColumnName = Properties.FirstOrDefault(p => p.IsCustomPrimaryKey)?.Name ?? PropertyData.DEFAULT_ID_COLUMN;

                return idColumnName;
            }
        }

        public bool HasAutoNumber
        {
            get
            {
                if (hasAutoNumber == null)
                    hasAutoNumber = Properties.Any(p => p.IsAutoNumber);

                return hasAutoNumber.Value;
            }
        }

        public IPropertyData AutoNumberProperty
        {
            get
            {
                if (HasAutoNumber == false) return null;

                if (autoNumberProperty == null)
                    autoNumberProperty = Properties.First(p => p.IsAutoNumber);

                return autoNumberProperty;
            }
        }

        public bool IsSoftDeleteEnabled { get; internal set; }

        internal DataProviderMetaData(Type type) => Type = type;
    }
}