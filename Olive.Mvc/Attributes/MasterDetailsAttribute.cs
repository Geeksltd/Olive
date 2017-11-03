using System;

namespace Olive.Mvc
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MasterDetailsAttribute : Attribute
    {
        public string Prefix { get; set; }

        public MasterDetailsAttribute(string prefix) => Prefix = prefix;
    }
}