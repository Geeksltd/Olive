using System;
using System.Reflection;

namespace Olive.Entities
{
    public interface IPropertyData
    {
        string Name { get; }
        string ParameterName { get; }
        bool IsAutoNumber { get; }
        bool IsDeleted { get; }
        bool IsUserDefined { get; }
        Type AssociateType { get; }
        bool IsDefaultId { get; }
        IPropertyAccessor Accessor { get; }
        PropertyInfo PropertyInfo { get; }
        bool IsCustomPrimaryKey { get; }
    }
}