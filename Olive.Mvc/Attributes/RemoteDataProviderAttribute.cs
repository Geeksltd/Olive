using System;

namespace Olive.Mvc
{
    /// <summary>
    /// When applied to a method, then Olive.ApiProxyGenerator will generate
    /// a data provider class that will be registered in consumer app, so this Api method can be invoked
    /// also using Database.Get().
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RemoteDataProviderAttribute : Attribute
    {
        public readonly Type EntityType;
        public RemoteDataProviderAttribute(Type entityType) => EntityType = entityType;
    }
}