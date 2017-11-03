using System;

namespace Olive.Mvc
{
    /// <summary>
    /// Specifies that a string property should not be trimmed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class KeepWhiteSpaceAttribute : Attribute { }
}
