using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.WebApi
{
    public static class Utilities
    {
        public static BadRequestObjectResult BadRequest(string message) => new BadRequestObjectResult(new { Message = message });

        public static NotFoundObjectResult NotFound(string message) => new NotFoundObjectResult(new { Message = message });
    }
}
