using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Olive.Mvc
{
    /// <summary>
    /// Intended for use on methods to specify the primary type of the result that is returned in happy scenarios.
    /// This is used in cases where the actual return type of the method itself can't be that type and has to be a generic interface to cater for exceptional scenarios (e.g. Web Api methods return IActionResult).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ReturnsAttribute : ProducesResponseTypeAttribute
    {
        public ReturnsAttribute(Type returnType) : base(returnType, StatusCodes.Status200OK)
        {
        }
    }
}