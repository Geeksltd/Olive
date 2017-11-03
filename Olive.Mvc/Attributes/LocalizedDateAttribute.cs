using System;

namespace Olive.Mvc
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class LocalizedDateAttribute : Attribute { }
}