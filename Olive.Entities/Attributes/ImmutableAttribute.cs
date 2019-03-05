using System;

namespace Olive.Entities
{
    /// <summary>
    /// When applied to a property, indicates that such property's value will never change and therefore it can be used for caching.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ImmutableAttribute : Attribute
    {
    }
}