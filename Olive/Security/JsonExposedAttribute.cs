using System;

namespace Olive
{
    /// <summary>Marks a property as Serializable (mainly for Json).</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class JsonExposedAttribute : Attribute { }
}