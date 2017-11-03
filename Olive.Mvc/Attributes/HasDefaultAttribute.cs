using System;

namespace Olive.Mvc
{
    /// <summary>
    /// Specifies that a ViewModel field has explicit default value that should be loaded for editing a new object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HasDefaultAttribute : Attribute { }
}
