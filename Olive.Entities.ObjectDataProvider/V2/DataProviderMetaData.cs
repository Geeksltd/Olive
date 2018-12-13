using System;
using System.Linq;

namespace Olive.Entities.ObjectDataProvider.V2
{
    public class DataProviderMetaData
    {
        Type[] BaseClassTypesInOrder;
        DataProviderMetaData[] baseClassesInOrder;
        
        Type[] DrivedClassTypesInOrder;
        DataProviderMetaData[] drivedClassesInOrder;

        public string TableName { get; set; }

        public string TableAlias { get; set; }

        public string Schema { get; set; }

        public PropertyData[] Properties { get; set; }

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
                    drivedClassesInOrder = DrivedClassTypesInOrder.Select(b => DataProviderMetaDataGenerator.Generate(b)).ToArray();

                return drivedClassesInOrder;
            }
        }

        public string IdColumnName => 
            Properties.FirstOrDefault(p => p.IsPrimaryKey)?.Name ?? PropertyData.DEFAULT_ID_COLUMN;

        internal DataProviderMetaData(Type type) => Type = type;
    }
}