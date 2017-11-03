using System;

namespace Olive.Mvc
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CopyDataAttribute : Attribute
    {
        public bool CanCopy;

        public CopyDataAttribute(bool canCopy) { CanCopy = canCopy; }
    }
}