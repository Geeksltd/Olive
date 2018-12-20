using System;
using System.Collections.Generic;
using System.Linq;

namespace Olive.Entities.Data
{
    public class DataProviderMetaData
    {
        DataProviderMetaData[] baseClassesInOrder;
        DataProviderMetaData[] drivedClassesInOrder;
        string idColumnName = null;
        bool? hasAutoNumber;
        PropertyData autoNumberProperty;

        public string TableName { get; internal set; }

        public string TableAlias { get; internal set; }

        public string Schema { get; internal set; }

        public PropertyData[] Properties { get; internal set; }

        public Type[] BaseClassTypesInOrder { get; internal set; }

        public Type[] DrivedClassTypes { get; internal set; }

        public Type Type { get; }

        public DataProviderMetaData[] BaseClassesInOrder
        {
            get
            {
                if (baseClassesInOrder == null)
                    baseClassesInOrder = BaseClassTypesInOrder.Select(b => DataProviderMetaDataGenerator.Generate(b)).ToArray();

                return baseClassesInOrder;
            }
        }

        public DataProviderMetaData[] DrivedClasses
        {
            get
            {
                if (drivedClassesInOrder == null)
                    drivedClassesInOrder = DrivedClassTypes.Select(b => DataProviderMetaDataGenerator.Generate(b)).ToArray();

                return drivedClassesInOrder;
            }
        }

        public IEnumerable<PropertyData> UserDefienedAndIdProperties => Properties?.Where(p => p.IsUserDefined || p.IsDefaultId);

        public IEnumerable<PropertyData> UserDefienedProperties => Properties?.Where(p => p.IsUserDefined);

        public IEnumerable<PropertyData> AssociateProperties => Properties?.Where(p => p.AssociateType != null);

        public string IdColumnName
        {
            get
            {
                if (idColumnName.IsEmpty())
                    idColumnName = Properties.FirstOrDefault(p => p.IsCustomPrimaryKey)?.Name ?? PropertyData.DEFAULT_ID_COLUMN;

                return idColumnName;
            }
        }

        public bool HasAutoNumber { get
            {
                if(hasAutoNumber == null)
                    hasAutoNumber = Properties.Any(p => p.IsAutoNumber);

                return hasAutoNumber.Value;
            }
        }

        public PropertyData AutoNumberProperty
        {
            get
            {
                if (HasAutoNumber == false) return null;

                if(autoNumberProperty == null)
                    autoNumberProperty = Properties.First(p => p.IsAutoNumber);

                return autoNumberProperty;
            }
        }

        public bool IsSoftDeleteEnabled { get; internal set; }

        internal DataProviderMetaData(Type type) => Type = type;
    }
}