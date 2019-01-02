using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olive.Entities
{
    public interface IDataProviderMetaData
    {
        IPropertyData[] Properties { get; }
        IEnumerable<IPropertyData> UserDefienedProperties { get; }
        IEnumerable<IPropertyData> UserDefienedAndIdProperties { get; }
        bool IsSoftDeleteEnabled { get; }
        string IdColumnName { get; }
        string TableName { get; }
        string Schema { get; }
        string TableAlias { get; }
        IDataProviderMetaData[] BaseClassesInOrder { get; }
        bool HasAutoNumber { get; }
        IPropertyData AutoNumberProperty { get; }
        Type Type { get; }
        IDataProviderMetaData[] DrivedClasses { get; }
        IEnumerable<IPropertyData> AssociateProperties { get; }
    }
}