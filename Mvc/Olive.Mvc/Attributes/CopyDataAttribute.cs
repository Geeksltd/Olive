namespace Olive.Mvc
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class CopyDataAttribute : Attribute
    {
        public bool CanCopy;

        public CopyDataAttribute(bool canCopy) { CanCopy = canCopy; }
    }
}