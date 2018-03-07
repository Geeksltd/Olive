using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Olive.Mvc
{
    /// <summary>
    /// When applied to a method, then Olive.ApiProxyGenerator will generate
    /// a data provider class that will be registered in consumer app, so this Api method can be invoked
    /// also using Database.Get().
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RemoteDataProvider : Attribute
    {
        Type EntityType;
        public RemoteDataProvider(Type entityType) => EntityType = entityType;

    }
}