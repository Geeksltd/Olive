using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities
{
    public interface IPropertyData
    {
        string Name{ get; }
        string ParameterName{ get; }
        bool IsAutoNumber { get; }
        bool IsDeleted { get; }
        bool IsUserDefined { get; }
        Type AssociateType { get; }
        bool IsDefaultId { get; }
        Action<IEntity, object> SetValue { get; }
        Func<IEntity, object> GetValue { get; }
        PropertyInfo PropertyInfo { get; }
        bool IsCustomPrimaryKey { get; }
    }
}