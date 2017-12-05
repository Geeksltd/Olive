namespace Olive.WebApi
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class MatchTypeAttribute : Attribute
    {
        public string Name { get; set; }
        public MatchTypeAttribute(string name) { Name = name; }
    }
}
