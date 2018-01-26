using System;

namespace Olive.Mvc
{
    /// <summary>
    /// Intended for use on Web Api methods to specify the primary type of the result that is returned in happy scenarios.
    /// The purpose of this attribute is to add the missing metadata information for code generators since Web Api methods
    /// return IActionResult.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ReturnsAttribute : Attribute
    {
        public readonly Type ReturnType;
        public ReturnsAttribute(Type returnType)
            => ReturnType = returnType ?? throw new ArgumentNullException();
    }
}