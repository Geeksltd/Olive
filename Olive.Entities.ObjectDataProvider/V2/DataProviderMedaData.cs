using System;
using System.Linq;

namespace Olive.Entities.ObjectDataProvider.V2
{
    public class DataProviderMedaData
    {
        Type[] BaseClassTypesInOrder;
        DataProviderMedaData[] baseClassesInOrder;
        readonly Type Type;
        Type[] DrivedClassTypesInOrder;
        DataProviderMedaData[] drivedClassesInOrder;

        public string TableName { get; set; }

        public string TableAlias { get; set; }

        public PropertyData[] Properties { get; set; }

        public DataProviderMedaData[] BaseClassesInOrder
        {
            get
            {
                if (baseClassesInOrder == null)
                    baseClassesInOrder = BaseClassTypesInOrder.Select(b => DataProviderMedaDataGenerator.Generate(b)).ToArray();

                return baseClassesInOrder;
            }
        }

        public DataProviderMedaData[] DrivedClasses
        {
            get
            {
                if (drivedClassesInOrder == null)
                    drivedClassesInOrder = DrivedClassTypesInOrder.Select(b => DataProviderMedaDataGenerator.Generate(b)).ToArray();

                return drivedClassesInOrder;
            }
        }

        internal DataProviderMedaData(Type type) => Type = type;
    }
}