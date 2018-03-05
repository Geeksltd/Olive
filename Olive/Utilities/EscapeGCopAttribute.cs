namespace Olive
{
    using System;

    /// <summary>
    /// When applied to a method it will skip all GCop warnings for that method (not to be abused).
    /// It is bad to escape any cop. Always try to avoid using this attribute by fixing your code.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class EscapeGCopAttribute : Attribute
    {
        public EscapeGCopAttribute(string reason = null) => Reason = reason;

        public string Reason { get; private set; }
    }
}