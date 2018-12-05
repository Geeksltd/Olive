using System;

namespace Olive.Entities
{
    /// <summary>
    /// When applied to a property, indicates the name property on the child side of the relationship.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class InverseOfAttribute : Attribute
    {
        public InverseOfAttribute(string association) => Association = association;

        public string Association { get; }
    }
}